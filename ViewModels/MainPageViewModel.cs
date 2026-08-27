using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlawlClient.Flawl.Api;
using FlawlClient.Flawl.DataModels;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient.ViewModels;

public partial class MainPageViewModel : ViewModelBase
{
    private static readonly ApiCaller ApiCaller = App.ServiceProvider?.GetRequiredService<ApiCaller>()!;
    [ObservableProperty] private ObservableCollection<ChatModel> _chats = ApiCaller.ChatApi.Chats;
    [ObservableProperty] private ObservableObject? _rightPanelContent;

    [ObservableProperty] private string _username = ApiCaller.UserApi.CurrentUser.Username;

    [RelayCommand]
    public async Task PerformLogout()
    {
        await ApiCaller.PerformLogout();
        App.ServiceProvider?.GetRequiredService<MainWindowViewModel>().Content =
            App.ServiceProvider.GetRequiredService<AuthPageViewModel>();
    }

    [RelayCommand]
    public void DisplayChat(ChatModel chatModel)
    {
        RightPanelContent = new ChatViewModel
        {
            ChatName = chatModel.Name,
            ChatId = chatModel.Id,
            Messages = chatModel.Messages
        };
    }
}