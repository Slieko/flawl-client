using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlawlClient.Flawl.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.ViewModels;

public partial class AuthPageViewModel : ViewModelBase
{
    private readonly ApiCaller _apiCaller = App.ServiceProvider?.GetRequiredService<ApiCaller>()!;
    private readonly ILogger _logger = App.ServiceProvider?.GetRequiredService<ILogger<AuthPageViewModel>>()!;
    [ObservableProperty] private bool _canInteract = true;

    [ObservableProperty] private string? _exception = "";
    [ObservableProperty] private string _host = "";
    [ObservableProperty] private string _login = "";
    [ObservableProperty] private string _password = "";

    [RelayCommand]
    public async Task ProvideAuthorization()
    {
        try
        {
            if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password))
            {
                Exception = "";

                CanInteract = false;
                await _apiCaller.Login(Login, Password, Host);
                CanInteract = true;

                App.ServiceProvider?.GetRequiredService<MainWindowViewModel>().Content =
                    App.ServiceProvider.GetRequiredService<MainPageViewModel>();
            }
            else
            {
                Exception = "";

                if (string.IsNullOrEmpty(Host)) Exception += "Server can't be empty!\n";
                if (string.IsNullOrEmpty(Login)) Exception += "Login can't be empty!\n";
                if (string.IsNullOrEmpty(Password)) Exception += "Password can't be empty!\n";

                _logger.LogError(Exception);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Exception = e.Message;
            CanInteract = true;
        }
    }

    [RelayCommand]
    public async Task ProvideRegistration()
    {
        try
        {
            if (!string.IsNullOrEmpty(Host) && !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password))
            {
                Exception = "";

                CanInteract = false;
                await _apiCaller.Register(Login, Password, Host);
                CanInteract = true;

                App.ServiceProvider?.GetRequiredService<MainWindowViewModel>().Content =
                    App.ServiceProvider.GetRequiredService<MainPageViewModel>();
            }
            else
            {
                Exception = "";

                if (string.IsNullOrEmpty(Host)) Exception += "Server can't be empty!\n";
                if (string.IsNullOrEmpty(Login)) Exception += "Login can't be empty!\n";
                if (string.IsNullOrEmpty(Password)) Exception += "Password can't be empty!\n";

                _logger.LogError(Exception);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            Exception = e.Message;
            CanInteract = true;
        }
    }
}