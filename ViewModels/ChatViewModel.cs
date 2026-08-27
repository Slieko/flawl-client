using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlawlClient.Flawl.Api;
using FlawlClient.Flawl.DataModels;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    private readonly ApiCaller _apiCaller = App.ServiceProvider?.GetRequiredService<ApiCaller>()!;
    public long ChatId;

    [ObservableProperty] private string? _chatName;
    [ObservableProperty] private string? _exception;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private ObservableCollection<MessageModel>? _messages;

    [RelayCommand]
    public async Task KeyPressed(KeyEventArgs e)
    {
        if (e.Key.Equals(Key.Return) && e.KeyModifiers.Equals(KeyModifiers.None))
        {
            e.Handled = true;
            if (!string.IsNullOrEmpty(Message))
            {
                Exception = "";
                var task = _apiCaller.ChatApi.SendMessage(ChatId, Message);
                Message = "";
                await task;
            }
        }
    }

    [RelayCommand]
    public void ScrollToBottom(RoutedEventArgs args)
    {
        if (args.Source is ScrollViewer scrollViewer)
            scrollViewer.Offset = new Vector(
                scrollViewer.Offset.X,
                scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
    }
}