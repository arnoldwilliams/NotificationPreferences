using NotificationPreferences.MauiClient.Views;

namespace NotificationPreferences.MauiClient;

public partial class AppShell : Shell
{
    public AppShell(NotificationPreferencePage notificationPreferencePage)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Notifications",
            Route = nameof(NotificationPreferencePage),
            Content = notificationPreferencePage
        });
    }
}
