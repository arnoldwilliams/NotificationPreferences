namespace NotificationPreferences.Contracts.Dtos;

/// <summary>
/// A single subscribable push notification topic, shaped for display as a child row
/// under its category group.
/// </summary>
public sealed record NotificationTopicDto
{
    public required int Id { get; init; }

    /// <summary>Stable identifier stored on the device once a user opts in or out.</summary>
    public required string Code { get; init; }

    public required string DisplayName { get; init; }

    public string? Description { get; init; }

    /// <summary>Category this topic belongs to; mirrors <see cref="NotificationCategoryDto.Id"/>.</summary>
    public required int CategoryId { get; init; }
}
