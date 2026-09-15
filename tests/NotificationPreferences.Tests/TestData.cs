using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Tests;

internal static class TestData
{
    public static NotificationCategory Category(
        int id,
        string code,
        string displayName,
        bool isActive = true,
        params NotificationTopic[] topics)
    {
        var category = new NotificationCategory
        {
            Id = id,
            Code = code,
            DisplayName = displayName,
            IsActive = isActive,
            CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        foreach (var topic in topics)
        {
            topic.CategoryId = id;
            category.Topics.Add(topic);
        }

        return category;
    }

    public static NotificationTopic Topic(
        int id,
        string code,
        string displayName,
        string? description = null,
        bool isActive = true) =>
        new()
        {
            Id = id,
            Code = code,
            DisplayName = displayName,
            Description = description,
            IsActive = isActive,
            CreatedUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        };
}
