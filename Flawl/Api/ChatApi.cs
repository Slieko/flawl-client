using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
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
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<ChatApi>>()!;
    private readonly RequestHelper _requestHelper = App.ServiceProvider?.GetRequiredService<RequestHelper>()!;
    private readonly UserApi _userApi = App.ServiceProvider?.GetRequiredService<UserApi>()!;
    public ObservableCollection<ChatModel> Chats = new();

    public async Task Initialize()
    {
        var tasks = new List<Task>();

        _logger.LogInformation($"Initializing {GetType().Name}...");
        Chats.Clear();

        var response = await _requestHelper.ChatRequest("/getChats", HttpMethod.Post);

        var chats = new ObservableCollection<ChatModel>(
            JsonSerializer.Deserialize<HashSet<ChatModel>>(await response.Content.ReadAsStringAsync())!);

        foreach (var chatModel in chats)
            tasks.Add(Task.Run(async () =>
            {
                await Task.WhenAll(GetChatMessages(chatModel), LoadChatImage(chatModel));

                Chats.Add(chatModel);
            }));

        await Task.WhenAll(tasks);

        Chats = new ObservableCollection<ChatModel>(Chats.OrderBy(c => c.Id).ToList());

        _logger.LogInformation($"Chats count: {Chats.Count}");
        _logger.LogInformation($"Successfully initialized {GetType().Name}");
    }

    public async Task LoadChatImage(ChatModel chatModel)
    {
        var response = await _requestHelper.GetChatAvatar(chatModel.Id);

        await using var avatar = await response.Content.ReadAsStreamAsync();

        try
        {
            chatModel.ChatImage = new Bitmap(avatar);
        }
        catch (Exception e)
        {
            _logger.LogError($"Unable to load avatar for Chat {chatModel.Id}. {e.Message}");
            chatModel.ChatImage =
                new Bitmap(AssetLoader.Open(new Uri("avares://FlawlClient/Assets/default_avatar.png")));
        }
    }

    public async Task LoadChatImage(long chatId)
    {
        await LoadChatImage(GetChat(chatId));
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

        GetChat(messageModel.ChatId).Messages.Add(messageModel);
    }

    public ChatModel GetChat(long chatId)
    {
        return Chats.First(chat => chat.Id == chatId);
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

    public async Task CreateChat(string name, HashSet<long> participants)
    {
        var content = new StringContent(JsonSerializer.Serialize(new ChatModel(name, participants)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await _requestHelper.ChatRequest("/create", HttpMethod.Post, content);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var error = json.GetProperty("error").GetString();
            _logger.LogError(error);
        }
    }

    public async Task UploadChatImage(Stream stream, long chatId)
    {
        var content = new MultipartFormDataContent();

        var imageContent = new StreamContent(stream);
        content.Add(imageContent, "file", "picture");

        var stringContent = new StringContent(JsonSerializer.Serialize(new
        {
            chatId
        }));
        stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        content.Add(stringContent, "chatId");

        var response = await _requestHelper.ChatRequest("/uploadAvatar", HttpMethod.Post, content);

        if (response.StatusCode != HttpStatusCode.OK)
        {
            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var error = json.GetProperty("error").GetString();
            _logger.LogError(error);
        }
    }
}