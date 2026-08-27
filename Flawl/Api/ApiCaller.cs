using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AdysTech.CredentialManager;
using FlawlClient.Flawl.DataModels;
using FlawlClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.Flawl.Api;

public class ApiCaller
{
    public readonly ChatApi ChatApi = App.ServiceProvider?.GetRequiredService<ChatApi>()!;
    public readonly EventHandler EventHandler = App.ServiceProvider?.GetRequiredService<EventHandler>()!;
    public readonly UserApi UserApi = App.ServiceProvider?.GetRequiredService<UserApi>()!;
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<ApiCaller>>()!;
    private readonly RequestHelper _requestHelper = App.ServiceProvider?.GetRequiredService<RequestHelper>()!;

    private ICredential? _credential;

    public bool IsCredentialPresent()
    {
        return _credential is not null;
    }

    private async Task SetupClient()
    {
        var response = await _requestHelper.GetAccessToken();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            _logger.LogInformation("Initializing other APIs");
            var start = DateTime.Now;

            await UserApi.Initialize(_credential!.UserName!);
            await ChatApi.Initialize();
            await Task.WhenAll(EventHandler.Initialize());
            _logger.LogInformation($"Done! Took {(DateTime.Now - start).TotalMilliseconds}ms");
        }
        else
        {
            var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
            var message = json.GetProperty("error").GetString();

            App.ServiceProvider?.GetRequiredService<AuthPageViewModel>().Exception = message;
            App.ServiceProvider?.GetRequiredService<MainWindowViewModel>().Content =
                App.ServiceProvider.GetRequiredService<AuthPageViewModel>();
            throw new Exception(message);
        }
    }

    public async Task Login(string username, string password, string host)
    {
        _requestHelper.Host = host;
        var content =
            new StringContent(JsonSerializer.Serialize(new LoginCredentialsModel(username, password)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await _requestHelper.AuthRequest("/login", HttpMethod.Post, content);
        await using var responseStream = await response.Content.ReadAsStreamAsync();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var refreshToken =
                await JsonSerializer.DeserializeAsync<RefreshTokenModel>(
                    responseStream);

            SaveCredential(username, host, refreshToken);
            _logger.LogInformation("Successfully logged in. Refresh token saved as " + _credential?.TargetName);

            await SetupClient();
        }
        else
        {
            var message = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream);
            throw new Exception(message.GetProperty("error").GetString());
        }
    }


    private void SaveCredential(string username, string host, RefreshTokenModel? refreshToken)
    {
        _credential = new NetworkCredential(username, refreshToken?.RefreshToken).ToICredential()!;
        _credential.TargetName = "FlawlClient:refreshToken";
        _credential.Attributes = new Dictionary<string, object>
        {
            { "host", host }
        };
        _credential.SaveCredential();
    }

    public async Task Register(string username, string password, string host)
    {
        _requestHelper.Host = host;
        var content = new StringContent(
            JsonSerializer.Serialize(new RegisterCredentialsModel(username, username, password)));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        using var response = await _requestHelper.AuthRequest("/register", HttpMethod.Post, content);
        await using var responseStream = await response.Content.ReadAsStreamAsync();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            await Login(username, password, host);
        }
        else
        {
            var message = await JsonSerializer.DeserializeAsync<JsonElement>(responseStream);
            throw new Exception(message.GetProperty("error").GetString());
        }
    }

    public async Task Initialize()
    {
        try
        {
            AppDomain.CurrentDomain.ProcessExit +=
                async (sender, args) => await EventHandler.CloseConnection("Client exit");

            _credential = CredentialManager.GetICredential("FlawlClient:refreshToken");

            if (IsCredentialPresent())
            {
                _requestHelper.Host = ((JsonElement)_credential?.Attributes?["host"]!).GetString();
                await SetupClient();
            }
        }
        catch (Exception e)
        {
            App.ServiceProvider?.GetRequiredService<AuthPageViewModel>().Exception = e.Message;

            _logger.LogError($"{e.Message}");
        }
    }

    public async Task PerformLogout()
    {
        try
        {
            await EventHandler.CloseConnection("Client logout");

            UserApi.LoadedUsers.Clear();
            ChatApi.Chats.Clear();

            CredentialManager.RemoveCredentials("FlawlClient:refreshToken");
            _logger.LogInformation("Successfully logged out");
        }
        catch (Exception e)
        {
            App.ServiceProvider?.GetRequiredService<AuthPageViewModel>().Exception = e.Message;

            _logger.LogError($"{e.Message}");
        }
    }
}