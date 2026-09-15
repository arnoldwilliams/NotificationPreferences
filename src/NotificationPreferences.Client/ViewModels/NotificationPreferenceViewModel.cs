using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotificationPreferences.Client.Models;
using NotificationPreferences.Client.Services;

namespace NotificationPreferences.Client.ViewModels;

public partial class NotificationPreferenceViewModel : ObservableObject
{
    private readonly INotificationPreferencesApiClient _apiClient;
    private readonly INotificationSubscriptionStore _subscriptionStore;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public NotificationPreferenceViewModel(
        INotificationPreferencesApiClient apiClient,
        INotificationSubscriptionStore subscriptionStore)
    {
        _apiClient = apiClient;
        _subscriptionStore = subscriptionStore;
    }

    public ObservableCollection<NotificationCategoryGroup> Groups { get; } = [];

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

    public bool IsEmpty => !IsBusy && !HasError && Groups.Count == 0;

    public bool CanSave => !IsBusy && Groups.Count > 0;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var categories = await _apiClient.GetCategoriesWithTopicsAsync();

            Groups.Clear();
            foreach (var category in categories)
            {
                var topics = category.Topics
                    .Select(topic => new TopicPreferenceViewModel(topic, _subscriptionStore))
                    .ToList();

                Groups.Add(new NotificationCategoryGroup(
                    category.Id,
                    category.Code,
                    category.DisplayName,
                    topics));
            }

            StatusMessage = Groups.Count == 0
                ? "No notification options are available right now."
                : string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Could not load notification options. Check your connection and try again.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Switch toggles are persisted by TopicPreferenceViewModel as they change, so there is
    // nothing to save here yet. Point this at the backend subscription endpoint once one
    // exists; until then it just confirms what was stored on the device.
    [RelayCommand]
    private void Save()
    {
        var subscribedCount = Groups
            .SelectMany(group => group)
            .Count(topic => topic.IsSubscribed);

        StatusMessage = subscribedCount == 0
            ? "All notifications are turned off."
            : $"Saved {subscribedCount} enabled {(subscribedCount == 1 ? "topic" : "topics")}.";
    }

    partial void OnErrorMessageChanged(string? value)
    {
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(IsEmpty));
    }

    partial void OnStatusMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasStatusMessage));
    }

    partial void OnIsBusyChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CanSave));
    }
}
