using CommunityToolkit.Mvvm.ComponentModel;
using NotificationPreferences.Contracts.Dtos;
using NotificationPreferences.Client.Services;

namespace NotificationPreferences.Client.ViewModels;

/// <summary>
/// A single topic row with the toggle the user flips. The DTO is immutable, so the
/// subscription state lives here instead of on the DTO.
/// </summary>
public partial class TopicPreferenceViewModel : ObservableObject
{
    private readonly INotificationSubscriptionStore _subscriptionStore;

    [ObservableProperty]
    private bool _isSubscribed;

    public TopicPreferenceViewModel(
        NotificationTopicDto topic,
        INotificationSubscriptionStore subscriptionStore)
    {
        _subscriptionStore = subscriptionStore;

        Id = topic.Id;
        Code = topic.Code;
        DisplayName = topic.DisplayName;
        Description = topic.Description;
        _isSubscribed = subscriptionStore.IsSubscribed(topic.Code);
    }

    public int Id { get; }

    public string Code { get; }

    public string DisplayName { get; }

    public string? Description { get; }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

    partial void OnIsSubscribedChanged(bool value)
    {
        _subscriptionStore.SetSubscribed(Code, value);
    }
}