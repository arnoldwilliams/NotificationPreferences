using NotificationPreferences.Client.Services;

namespace NotificationPreferences.MauiClient.Tests;

/// <summary>
/// In-memory <see cref="INotificationSubscriptionStore"/> used to exercise the view models
/// without a platform preference store.
/// </summary>
internal sealed class FakeSubscriptionStore : INotificationSubscriptionStore
{
    private readonly Dictionary<string, bool> _values = [];

    public int WriteCount { get; private set; }

    public FakeSubscriptionStore(params string[] subscribedCodes)
    {
        foreach (var code in subscribedCodes)
        {
            _values[code] = true;
        }
    }

    public bool IsSubscribed(string topicCode) => _values.TryGetValue(topicCode, out var value) && value;

    public void SetSubscribed(string topicCode, bool isSubscribed)
    {
        WriteCount++;
        _values[topicCode] = isSubscribed;
    }
}
