namespace NotificationPreferences.Api.Data.Entities;

/// <summary>
/// Maps to dbo.NotificationTopics. A single opt-in/opt-out push notification topic.
/// </summary>
public class NotificationTopic
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; }

    public NotificationCategory Category { get; set; } = null!;
}
