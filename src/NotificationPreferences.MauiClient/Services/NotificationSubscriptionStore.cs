using NotificationPreferences.Client.Services;

namespace NotificationPreferences.MauiClient.Services;

/// <summary>
/// Persists per-topic opt-in state in MAUI's platform preference store.
/// </summary>
/// <remarks>
/// This is device-local storage, not the source of truth for the user's account. A production
/// build should also sync these codes to the backend (and to the push provider's topic
/// registration) so the choice follows the user across devices and reinstallations.
/// </remarks>
public sealed class NotificationSubscriptionStore : INotificationSubscriptionStore
{
    private const string KeyPrefix = "notification_topic_subscribed:";

    public bool IsSubscribed(string topicCode)
    {
        return Preferences.Default.Get(Key(topicCode), false);
    }

    public void SetSubscribed(string topicCode, bool isSubscribed)
    {
        Preferences.Default.Set(Key(topicCode), isSubscribed);
    }

    private static string Key(string topicCode) => KeyPrefix + topicCode;
}
