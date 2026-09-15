namespace NotificationPreferences.Api.Data.Entities;

/// <summary>
/// Maps to dbo.NotificationCategories. A grouping of related notification topics.
/// </summary>
public class NotificationCategory
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; }

    public ICollection<NotificationTopic> Topics { get; set; } = new List<NotificationTopic>();
}
