using Microsoft.Extensions.Logging;
using NotificationPreferences.Client.Services;
using NotificationPreferences.MauiClient.Services;
using NotificationPreferences.Client.ViewModels;
using NotificationPreferences.MauiClient.Views;

namespace NotificationPreferences.MauiClient;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<INotificationSubscriptionStore, NotificationSubscriptionStore>();

        builder.Services.AddHttpClient<INotificationPreferencesApiClient, NotificationPreferencesApiClient>(
            client => client.BaseAddress = new Uri(ApiConfiguration.BaseAddress));

        builder.Services.AddTransient<NotificationPreferenceViewModel>();
        builder.Services.AddTransient<NotificationPreferencePage>();
        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}