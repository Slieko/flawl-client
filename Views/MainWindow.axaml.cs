using Avalonia.Controls;
using FlawlClient.Flawl.Api;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override async void OnInitialized()
    {
        base.OnInitialized();
        await App.ServiceProvider?.GetRequiredService<ApiCaller>().Initialize()!;
    }
}