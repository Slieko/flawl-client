using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using FlawlClient.Flawl.DataModels;
using FlawlClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.Flawl.Api;

public class ChatApi
{
    public readonly ObservableCollection<ChatModel> Chats = new();
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<ChatApi>>()!;
    private readonly RequestHelper _requestHelper = App.ServiceProvider?.GetRequiredService<RequestHelper>()!;
    private readonly UserApi _userApi = App.ServiceProvider?.GetRequiredService<UserApi>()!;

    public async Task Initialize()
    {
        _logger.LogInformation($"Initializing {GetType().Name}...");
        Chats.Clear();

        var response = await _requestHelper.ChatRequest("/getChats", HttpMethod.Post);

        var chats = new ObservableCollection<ChatModel>(
            JsonSerializer.Deserialize<HashSet<ChatModel>>(await response.Content.ReadAsStringAsync())!);

        foreach (var chatModel in chats)
        {
            await GetChatMessages(chatModel);
            await GenerateChatImage(chatModel);

            Chats.Add(chatModel);
        }

        _logger.LogInformation($"Chats count: {Chats.Count}");
        _logger.LogInformation($"Successfully initialized {GetType().Name}");
    }

    private async Task GenerateChatImage(ChatModel chatModel)
    {
        try
        {
            List<IImage> avatars = new();

            foreach (var participantId in chatModel.ParticipantsIds)
                if (participantId != _userApi.CurrentUser.UserId)
                    avatars.Add(_userApi.GetUser(participantId).Avatar);

            chatModel.ChatImage = avatars.First();
        }
        catch (Exception e)
        {
            chatModel.ChatImage =
                new Bitmap(AssetLoader.Open(new Uri("avares://FlawlClient/Assets/default_avatar.png")));
            _logger.LogError(e.Message);
        }
    }

    private async Task GetChatMessages(ChatModel chatModel)
    {
        var response = await _requestHelper.GetMessages(chatModel.Id);

        var messageModels =
            JsonSerializer.Deserialize<List<MessageModel>>(await response.Content.ReadAsStringAsync())!;

        foreach (var messageModel in messageModels)
        {
            await ProcessMessage(messageModel);

            chatModel.Messages.Add(messageModel);
        }

        _logger.LogInformation($"Loaded {messageModels.Count} messages for chat {chatModel.Id} ({chatModel.Name})");
    }

    public async Task AddMessage(MessageModel messageModel)
    {
        await ProcessMessage(messageModel);

        Chats.First(chat => chat.Id.Equals(messageModel.ChatId)).Messages.Add(messageModel);
    }

    private async Task ProcessMessage(MessageModel model)
    {
        await _userApi.LoadUser(model.SenderId);

        model.SenderModel = _userApi.GetUser(model.SenderId);

        model.SendTime = model.SendTime.ToLocalTime();
    }

    public async Task SendMessage(long chatId, string message)
    {
        using var content = new MultipartFormDataContent();

        using var textContent = new StringContent(JsonSerializer.Serialize(new
        {
            chatId,
            content = message
        }));
        textContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        content.Add(textContent, "message");

        var response = await _requestHelper.ChatRequest("/send", HttpMethod.Post, content);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var json = await JsonSerializer.DeserializeAsync<JsonElement>(await response.Content.ReadAsStreamAsync());
            var error = json.GetProperty("error").GetString();

            Dispatcher.UIThread.Post(() =>
            {
                if (App.ServiceProvider?.GetRequiredService<MainPageViewModel>().RightPanelContent is ChatViewModel
                    chatViewModel)
                    chatViewModel.Exception = error;
            });
        }
    }
}