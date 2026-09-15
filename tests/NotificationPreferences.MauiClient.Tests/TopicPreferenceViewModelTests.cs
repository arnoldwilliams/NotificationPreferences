using NotificationPreferences.Client.Services;
using NotificationPreferences.Client.ViewModels;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.MauiClient.Tests;

public class TopicPreferenceViewModelTests
{
    [Fact]
    public void ReflectsStoredSubscriptionStateOnConstruction()
    {
        var store = new FakeSubscriptionStore();

        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts"),
            store);

        Assert.False(viewModel.IsSubscribed);
    }

    [Fact]
    public void OnlyPreloadsTheTopicThatWasPreviouslySubscribed()
    {
        var store = new FakeSubscriptionStore("account.statements");

        var subscribed = new TopicPreferenceViewModel(
            TestData.Topic(11, 1, "account.statements", "Statements"), store);
        var other = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts"), store);

        Assert.True(subscribed.IsSubscribed);
        Assert.False(other.IsSubscribed);
    }

    [Fact]
    public void PersistsToggleOnUnderTheTopicCode()
    {
        var store = new FakeSubscriptionStore();
        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts"),
            store);

        viewModel.IsSubscribed = true;

        Assert.True(store.IsSubscribed("account.security"));
        Assert.Equal(1, store.WriteCount);
    }

    [Fact]
    public void PersistsToggleOffForATopicThatWasPreviouslySubscribed()
    {
        var store = new FakeSubscriptionStore("account.security");
        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts"),
            store);

        viewModel.IsSubscribed = false;

        Assert.False(store.IsSubscribed("account.security"));
        Assert.Equal(1, store.WriteCount);
    }

    [Fact]
    public void DoesNotWriteToStorageWhenTheValueIsUnchanged()
    {
        var store = new FakeSubscriptionStore("account.security");
        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts"),
            store);

        Assert.True(viewModel.IsSubscribed);
        Assert.Equal(0, store.WriteCount);
    }

    [Fact]
    public void CarriesCodeAndDisplayFieldsFromTheDto()
    {
        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(10, 1, "account.security", "Security alerts", "Sign-ins and password changes."),
            new FakeSubscriptionStore());

        Assert.Equal(10, viewModel.Id);
        Assert.Equal("account.security", viewModel.Code);
        Assert.Equal("Security alerts", viewModel.DisplayName);
        Assert.Equal("Sign-ins and password changes.", viewModel.Description);
        Assert.True(viewModel.HasDescription);
    }

    [Fact]
    public void TopicWithoutDescriptionHidesTheDescriptionRow()
    {
        var viewModel = new TopicPreferenceViewModel(
            TestData.Topic(11, 1, "account.statements", "Statements"),
            new FakeSubscriptionStore());

        Assert.Null(viewModel.Description);
        Assert.False(viewModel.HasDescription);
    }
}
