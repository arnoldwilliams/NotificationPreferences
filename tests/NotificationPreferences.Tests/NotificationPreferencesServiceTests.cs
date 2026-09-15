using NotificationPreferences.Api.Repositories;
using NotificationPreferences.Api.Services;

namespace NotificationPreferences.Tests;

public class NotificationPreferencesServiceTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    [Fact]
    public async Task GetCategoryTopicsAsync_MapsEntitiesToDtosWithSortedTopics()
    {
        using var context = _factory.CreateSeededContext(
        [
            TestData.Category(1, "SECURITY", "Security", topics:
            [
                TestData.Topic(10, "LOGIN_ALERTS", "Zulu topic", "Login activity"),
                TestData.Topic(11, "PASSWORD_CHANGES", "Alpha topic"),
            ]),
        ]);

        var service = new NotificationPreferencesService(new NotificationPreferencesRepository(context));

        var result = await service.GetCategoryTopicsAsync();

        var category = Assert.Single(result);
        Assert.Equal(1, category.Id);
        Assert.Equal("SECURITY", category.Code);
        Assert.Equal("Security", category.DisplayName);

        Assert.Equal(["Alpha topic", "Zulu topic"], category.Topics.Select(t => t.DisplayName));

        var topic = category.Topics[1];
        Assert.Equal(10, topic.Id);
        Assert.Equal("LOGIN_ALERTS", topic.Code);
        Assert.Equal("Login activity", topic.Description);
        Assert.Equal(1, topic.CategoryId);
    }

    [Fact]
    public async Task GetCategoryTopicsAsync_ReturnsEmptyWhenNoCategoriesExist()
    {
        using var context = _factory.CreateContext();
        var service = new NotificationPreferencesService(new NotificationPreferencesRepository(context));

        var result = await service.GetCategoryTopicsAsync();

        Assert.Empty(result);
    }

    public void Dispose() => _factory.Dispose();
}
