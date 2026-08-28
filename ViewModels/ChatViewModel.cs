using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
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

    public IImage? ChatImage { get; set; }
    public string? ChatName { get; set; }

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

    [RelayCommand]
    public async Task ChangeAvatar(RoutedEventArgs args)
    {
        if (args.Source is Visual visual)
        {
            var topLevel = TopLevel.GetTopLevel(visual)!;

            var file = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                SuggestedFileType = FilePickerFileTypes.ImageAll,
                Title = "Choose an image",
                AllowMultiple = false
            });

            if (file.Count >= 1)
            {
                await using var stream = await file[0].OpenReadAsync();
                await _apiCaller.ChatApi.UploadChatImage(stream, ChatId);
            }
        }
    }
}