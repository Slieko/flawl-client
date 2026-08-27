using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FlawlClient.Flawl;
using FlawlClient.ViewModels;
using FlawlClient.Views;
using Microsoft.Extensions.DependencyInjection;

namespace FlawlClient;

public class App : Application
{
    private static readonly IServiceCollection Services = new ServiceCollection();
    public static ServiceProvider? ServiceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services
            .AddViewModels()
            .AddApiClasses();

        ServiceProvider = Services.BuildServiceProvider();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };

        base.OnFrameworkInitializationCompleted();
    }
}