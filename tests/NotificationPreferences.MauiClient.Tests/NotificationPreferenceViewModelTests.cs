using NotificationPreferences.Client.ViewModels;

namespace NotificationPreferences.MauiClient.Tests;

public class NotificationPreferenceViewModelTests
{
    [Fact]
    public async Task LoadGroupsTopicsUnderTheirCategoryInApiOrder()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Groups.Count);
        Assert.Equal("Account", viewModel.Groups[0].DisplayName);
        Assert.Equal("MARKETING", viewModel.Groups[1].Code);
        Assert.Equal(
            ["account.security", "account.statements"],
            viewModel.Groups[0].Select(topic => topic.Code));
    }

    [Fact]
    public async Task LoadWrapsEachTopicInItsOwnToggle()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore("marketing.promotions"));

        await viewModel.LoadCommand.ExecuteAsync(null);

        var marketingTopic = Assert.Single(viewModel.Groups[1]);
        Assert.True(marketingTopic.IsSubscribed);
    }

    [Fact]
    public async Task LoadClearsBusyAndErrorStateOnSuccess()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsBusy);
        Assert.False(viewModel.HasError);
        Assert.False(viewModel.IsEmpty);
        Assert.True(viewModel.CanSave);
        Assert.Empty(viewModel.StatusMessage);
    }

    [Fact]
    public async Task LoadSurfacesARetryableErrorWithoutThrowing()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(new HttpRequestException("connection refused")),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.IsBusy);
        Assert.True(viewModel.HasError);
        Assert.Contains("try again", viewModel.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(viewModel.Groups);
        Assert.False(viewModel.CanSave);
    }

    [Fact]
    public async Task SuccessfulReloadReplacesPreviousGroupsInsteadOfAppending()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);
        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(2, viewModel.Groups.Count);
        Assert.Equal(3, viewModel.Groups.Sum(group => group.Count));
    }

    [Fact]
    public async Task EmptyResponseReportsNoOptionsAndDisablesSave()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient([]),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Empty(viewModel.Groups);
        Assert.True(viewModel.HasStatusMessage);
        Assert.False(viewModel.CanSave);
    }

    [Fact]
    public async Task GroupReportsItsTopicCountSingularAndPlural()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal("2 topics", viewModel.Groups[0].TopicCountLabel);
        Assert.Equal("1 topic", viewModel.Groups[1].TopicCountLabel);
    }

    [Fact]
    public async Task SaveSummarisesTheEnabledTopicCount()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.Groups[0][0].IsSubscribed = true;
        viewModel.SaveCommand.Execute(null);

        Assert.Contains("1 enabled topic", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveReportsWhenEverythingIsTurnedOff()
    {
        var viewModel = new NotificationPreferenceViewModel(
            new StubApiClient(TestData.CategoriesWithTopics()),
            new FakeSubscriptionStore());
        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SaveCommand.Execute(null);

        Assert.Contains("turned off", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RepeatedLoadIsIgnoredWhileOneIsInFlight()
    {
        var apiClient = new StubApiClient(TestData.CategoriesWithTopics())
        {
            Gate = new TaskCompletionSource()
        };
        var viewModel = new NotificationPreferenceViewModel(apiClient, new FakeSubscriptionStore());

        var first = viewModel.LoadCommand.ExecuteAsync(null);
        var second = viewModel.LoadCommand.ExecuteAsync(null);

        apiClient.Gate.SetResult();
        await Task.WhenAll(first, second);

        Assert.Equal(1, apiClient.CallCount);
        Assert.Equal(2, viewModel.Groups.Count);
    }
}
