using CommunityToolkit.Mvvm.ComponentModel;
using FlawlClient.Flawl.Api;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableObject _content =
        App.ServiceProvider!.GetRequiredService<ApiCaller>().IsCredentialPresent()
            ? App.ServiceProvider?.GetRequiredService<MainPageViewModel>()!
            : App.ServiceProvider?.GetRequiredService<AuthPageViewModel>()!;
}