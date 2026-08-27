using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FlawlClient.Flawl.DataModels;

namespace FlawlClient.Flawl.Api;

public class RequestHelper
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(45)
    };

    public string? Host;
    public AccessTokenModel? JwtToken;

    public async ValueTask<HttpResponseMessage> GetAccessToken()
    {
        var content =
            new StringContent(
                JsonSerializer.Serialize(new RefreshTokenModel(
                    CredentialManager.GetICredential("FlawlClient:refreshToken")!.ToNetworkCredential().Password)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var response = await AuthRequest("/refresh", HttpMethod.Post, content);

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        JwtToken = await JsonSerializer.DeserializeAsync<AccessTokenModel>(responseStream);

        return response;
    }

    private async ValueTask<HttpResponseMessage> Request(string basePath, string path, HttpMethod method,
        HttpContent? httpContent = null)
    {
        using HttpRequestMessage httpRequestMessage = new();
        httpRequestMessage.Content = httpContent;
        httpRequestMessage.RequestUri = new Uri(Host + "/api/v1" + basePath + path);
        httpRequestMessage.Method = method;

        if (JwtToken is not null)
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken.AccessToken);

        var response = await _httpClient.SendAsync(httpRequestMessage);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await GetAccessToken();
            return await Request(basePath, path, method, httpContent);
        }

        return response;
    }

    public async ValueTask<HttpResponseMessage> AuthRequest(string path, HttpMethod method,
        HttpContent? httpContent = null)
    {
        return await Request("/auth", path, method, httpContent);
    }

    public async ValueTask<HttpResponseMessage> UserRequest(string path, HttpMethod method,
        HttpContent? httpContent = null)
    {
        return await Request("/users", path, method, httpContent);
    }

    public async ValueTask<HttpResponseMessage> ChatRequest(string path, HttpMethod method,
        HttpContent? httpContent = null)
    {
        return await Request("/chat", path, method, httpContent);
    }

    private async ValueTask<HttpResponseMessage> StorageRequest(string path, HttpMethod method,
        HttpContent? httpContent = null)
    {
        return await Request("/storage", path, method, httpContent);
    }

    public async ValueTask<HttpResponseMessage> GetAvatar(long userId)
    {
        return await StorageRequest($"/avatars/{userId}", HttpMethod.Get);
    }

    public async ValueTask<HttpResponseMessage> GetAttachment(string attachment)
    {
        return await StorageRequest($"/attachments/{attachment}", HttpMethod.Get);
    }

    public async ValueTask<HttpResponseMessage> GetChat(long chatId)
    {
        var content = new StringContent(JsonSerializer.Serialize(new
        {
            chatId
        }));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return await ChatRequest("/getChat", HttpMethod.Post, content);
    }

    public async ValueTask<HttpResponseMessage> GetMessage(long messageId)
    {
        var content = new StringContent(JsonSerializer.Serialize(new
        {
            messageId
        }));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return await ChatRequest("/getMessage", HttpMethod.Post, content);
    }

    public async ValueTask<HttpResponseMessage> GetMessages(long chatId)
    {
        var content = new StringContent(JsonSerializer.Serialize(new
        {
            chatId
        }));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return await ChatRequest("/getMessages", HttpMethod.Post, content);
    }
}