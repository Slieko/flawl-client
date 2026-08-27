using FlawlClient.Flawl.Api;
using FlawlClient.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlawlClient.Flawl;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddViewModels(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<AuthPageViewModel>();
        services.AddSingleton<MainPageViewModel>();
        return services;
    }

    public static IServiceCollection AddApiClasses(this IServiceCollection services)
    {
        services.AddSingleton<ApiCaller>();
        services.AddSingleton<UserApi>();
        services.AddSingleton<ChatApi>();
        services.AddSingleton<EventHandler>();
        services.AddSingleton<RequestHelper>();
        services.AddLogging(builder => builder.AddSimpleConsole(options => options.SingleLine = true));

        return services;
    }
}