using NotificationPreferences.Api.Repositories;

namespace NotificationPreferences.Tests;

public class NotificationPreferencesRepositoryTests : IDisposable
{
    private readonly TestDbContextFactory _factory = new();

    private static readonly string[] ExpectedCodes = ["ACCOUNT", "SECURITY"];

    [Fact]
    public async Task GetCategoriesWithTopicsAsync_ReturnsOnlyActiveCategoriesAndTopics()
    {
        using var context = _factory.CreateSeededContext(
        [
            TestData.Category(1, "SECURITY", "Security", topics:
            [
                TestData.Topic(10, "LOGIN_ALERTS", "Login alerts"),
                TestData.Topic(11, "RETIRED_TOPIC", "Retired", isActive: false),
            ]),
            TestData.Category(2, "MARKETING", "Marketing", isActive: false, topics:
            [
                TestData.Topic(12, "PROMOS", "Promotions"),
            ]),
            TestData.Category(3, "EMPTY", "Empty category", topics:
            [
                TestData.Topic(13, "ALL_OFF", "Everything off", isActive: false),
            ]),
        ]);

        var repository = new NotificationPreferencesRepository(context);

        var result = await repository.GetCategoriesWithTopicsAsync();

        var category = Assert.Single(result);
        Assert.Equal("Security", category.DisplayName);

        var topic = Assert.Single(category.Topics);
        Assert.Equal("LOGIN_ALERTS", topic.Code);
    }

    [Fact]
    public async Task GetCategoriesWithTopicsAsync_OrdersCategoriesByDisplayName()
    {
        using var context = _factory.CreateSeededContext(
        [
            TestData.Category(1, "SECURITY", "Security", topics: [TestData.Topic(10, "A", "A")]),
            TestData.Category(2, "ACCOUNT", "Account", topics: [TestData.Topic(11, "B", "B")]),
        ]);

        var repository = new NotificationPreferencesRepository(context);

        var result = await repository.GetCategoriesWithTopicsAsync();

        Assert.Equal(ExpectedCodes.Length, result.Count);
        Assert.Equal(["Account", "Security"], result.Select(c => c.DisplayName));
    }

    [Fact]
    public async Task GetCategoriesWithTopicsAsync_ReturnsEmptyWhenNothingIsActive()
    {
        using var context = _factory.CreateSeededContext(
        [
            TestData.Category(1, "SECURITY", "Security", isActive: false, topics: [TestData.Topic(10, "A", "A")]),
        ]);

        var repository = new NotificationPreferencesRepository(context);

        var result = await repository.GetCategoriesWithTopicsAsync();

        Assert.Empty(result);
    }

    public void Dispose() => _factory.Dispose();
}
