namespace NotificationPreferences.Contracts.Dtos;

/// <summary>
/// A notification category together with the topics it contains. This is the unit of
/// data a grouped CollectionView binds to (category = group header, topics = children).
/// </summary>
public sealed record NotificationCategoryDto
{
    public required int Id { get; init; }

    public required string Code { get; init; }

    public required string DisplayName { get; init; }

    /// <summary>
    /// Active topics for this category, ordered by <see cref="NotificationTopicDto.DisplayName"/>.
    /// Categories with no active topics are omitted from the response entirely.
    /// </summary>
    public required IReadOnlyList<NotificationTopicDto> Topics { get; init; }
}
