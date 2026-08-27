using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using FlawlClient.Flawl.DataModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.Flawl.Api;

public class UserApi
{
    public readonly List<UserModel> LoadedUsers = new();
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<UserApi>>()!;
    private readonly RequestHelper _requestHelper = App.ServiceProvider?.GetRequiredService<RequestHelper>()!;
    public required UserModel CurrentUser { get; set; }

    public async Task Initialize(string username)
    {
        _logger.LogInformation($"Initializing {GetType().Name}...");
        var response = await _requestHelper.UserRequest($"/getUser/{username}", HttpMethod.Get);

        CurrentUser = JsonSerializer.Deserialize<UserModel>(await response.Content.ReadAsStringAsync())!;

        await LoadUserAvatar(CurrentUser);
        LoadedUsers.Add(CurrentUser);

        _logger.LogInformation($"Setting up current user: {CurrentUser.Username} ({CurrentUser.UserId})");
        _logger.LogInformation($"Successfully initialized {GetType().Name}");
    }

    private async Task LoadUserAvatar(UserModel userModel)
    {
        var response = await _requestHelper.GetAvatar(userModel.UserId);

        await using var avatar = await response.Content.ReadAsStreamAsync();

        try
        {
            IImage image = new Bitmap(avatar);

            userModel.Avatar = image;
        }
        catch (Exception e)
        {
            _logger.LogError($"Unable to load avatar for User {userModel.UserId}. {e.Message}");
            userModel.Avatar = new Bitmap(AssetLoader.Open(new Uri("avares://FlawlClient/Assets/default_avatar.png")));
        }
    }

    public async Task LoadUser(long userId)
    {
        if (IsUserLoaded(userId)) return;

        var response = await _requestHelper.UserRequest($"/getUserById/{userId}", HttpMethod.Get);
        var userModel = JsonSerializer.Deserialize<UserModel>(await response.Content.ReadAsStringAsync())!;

        await Task.WhenAll(LoadUserAvatar(userModel));

        LoadedUsers.Add(userModel);
    }

    public void UnloadUser(UserModel userModel)
    {
        LoadedUsers.Remove(userModel);
    }

    public bool IsUserLoaded(long userId)
    {
        return LoadedUsers.Any(model => model.UserId == userId);
    }

    public UserModel GetUser(long userId)
    {
        return LoadedUsers.First(model => model.UserId == userId);
    }
}