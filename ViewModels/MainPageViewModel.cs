using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlawlClient.Flawl.Api;
using FlawlClient.Flawl.DataModels;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient.ViewModels;

public partial class MainPageViewModel : ViewModelBase
{
    private static readonly ApiCaller ApiCaller = App.ServiceProvider?.GetRequiredService<ApiCaller>()!;

    [ObservableProperty] private ObservableCollection<ChatModel>? _chats;
    [ObservableProperty] private UserModel? _currentUser;
    [ObservableProperty] private ObservableObject? _rightPanelContent;

    [RelayCommand]
    public async Task PerformLogout()
    {
        await ApiCaller.PerformLogout();
        App.ServiceProvider?.GetRequiredService<MainWindowViewModel>().Content =
            App.ServiceProvider.GetRequiredService<AuthPageViewModel>();
    }

    [RelayCommand]
    public void DisplayChat(SelectionChangedEventArgs args)
    {
        if (args.AddedItems[0] is ChatModel chatModel)
            RightPanelContent = new ChatViewModel
            {
                ChatName = chatModel.Name,
                ChatId = chatModel.Id,
                Messages = chatModel.Messages,
                ChatImage = chatModel.ChatImage
            };
    }

    [RelayCommand]
    public async Task Test()
    {
        // await ApiCaller.ChatApi.CreateChat("test", [1]);
    }
}