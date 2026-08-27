using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FlawlClient.Flawl.DataModels;
using FlawlClient.Flawl.DataModels.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.Flawl.Api;

public class EventHandler
{
    private readonly ChatApi _chatApi = App.ServiceProvider?.GetRequiredService<ChatApi>()!;
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<EventHandler>>()!;
    private readonly RequestHelper _requestHelper = App.ServiceProvider?.GetRequiredService<RequestHelper>()!;
    private ClientWebSocket? _webSocket;

    public async Task Initialize()
    {
        _webSocket = new ClientWebSocket();
        _logger.LogInformation($"Initializing {GetType().Name}...");
        _logger.LogInformation("Trying establish websocket connection...");
        _webSocket.Options.SetRequestHeader("Authorization", "Bearer " + _requestHelper.JwtToken?.AccessToken);

        var task = Connect();
    }

    private async Task Connect()
    {
        await _webSocket!.ConnectAsync(new Uri(Regex.Replace(_requestHelper.Host!, "http[s]?", "ws") + "/api/v1/ws"),
            CancellationToken.None);
        _logger.LogInformation("Connection established");

        var buffer = new Memory<byte>(ArrayPool<byte>.Shared.Rent(1024));

        while (_webSocket.State.Equals(WebSocketState.Open))
        {
            var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (result.EndOfMessage)
                await HandleEvent(
                    JsonSerializer.Deserialize<Event>(Encoding.UTF8.GetString(buffer.Span[..result.Count])));
        }
    }

    public async Task CloseConnection(string? reason)
    {
        await _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None)!;
        _logger.LogInformation($"Closed websocket, reason: {reason}");
    }

    private async Task HandleEvent(Event? @event)
    {
        if (@event is null) return;

        if (@event is MessageSentEvent messageSentEvent)
        {
            var response = await _requestHelper.GetMessage(messageSentEvent.MessageId);
            var message = JsonSerializer.Deserialize<MessageModel>(await response.Content.ReadAsStringAsync())!;

            await _chatApi.AddMessage(message);
        }
    }
}