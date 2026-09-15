namespace NotificationPreferences.Client.Services;

public interface INotificationSubscriptionStore
{
    bool IsSubscribed(string topicCode);

    void SetSubscribed(string topicCode, bool isSubscribed);
}
