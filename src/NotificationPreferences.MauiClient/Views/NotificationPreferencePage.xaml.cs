using NotificationPreferences.Client.ViewModels;

namespace NotificationPreferences.MauiClient.Views;

public partial class NotificationPreferencePage : ContentPage
{
    private readonly NotificationPreferenceViewModel _viewModel;

    public NotificationPreferencePage(NotificationPreferenceViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // LoadCommand is a no-op while a load is already running, so re-entering the page
        // does not stack requests.
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }
}
