using NotificationPreferences.Client.Services;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.MauiClient.Tests;

/// <summary>
/// Serves a canned response, or a failure, so the view model can be driven end to end.
/// </summary>
internal sealed class StubApiClient : INotificationPreferencesApiClient
{
    private readonly IReadOnlyList<NotificationCategoryDto> _categories;
    private readonly Exception? _exception;

    public StubApiClient(IReadOnlyList<NotificationCategoryDto> categories)
    {
        _categories = categories;
    }

    public StubApiClient(Exception exception)
    {
        _categories = [];
        _exception = exception;
    }

    public int CallCount { get; private set; }

    /// <summary>When set, the response is withheld until the test releases it.</summary>
    public TaskCompletionSource? Gate { get; set; }

    public async Task<IReadOnlyList<NotificationCategoryDto>> GetCategoriesWithTopicsAsync(
        CancellationToken cancellationToken = default)
    {
        CallCount++;

        if (Gate is not null)
        {
            await Gate.Task;
        }

        if (_exception is not null)
        {
            throw _exception;
        }

        return _categories;
    }
}

internal static class TestData
{
    public static NotificationTopicDto Topic(
        int id,
        int categoryId,
        string code,
        string displayName,
        string? description = null) =>
        new()
        {
            Id = id,
            CategoryId = categoryId,
            Code = code,
            DisplayName = displayName,
            Description = description
        };

    public static NotificationCategoryDto Category(
        int id,
        string code,
        string displayName,
        params NotificationTopicDto[] topics) =>
        new()
        {
            Id = id,
            Code = code,
            DisplayName = displayName,
            Topics = topics
        };

    /// <summary>Shape mirrors the seeded schema: two categories, three topics total.</summary>
    public static IReadOnlyList<NotificationCategoryDto> CategoriesWithTopics() =>
    [
        Category(1, "ACCOUNT", "Account",
            Topic(10, 1, "account.security", "Security alerts", "Sign-ins and password changes."),
            Topic(11, 1, "account.statements", "Statements")),
        Category(2, "MARKETING", "Marketing",
            Topic(20, 2, "marketing.promotions", "Promotions & offers", "Discounts and campaigns."))
    ];
}
