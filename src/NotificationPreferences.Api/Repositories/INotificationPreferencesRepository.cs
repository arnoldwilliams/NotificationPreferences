using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Api.Repositories;

/// <summary>
/// Read access to the notification category/topic reference data used to render the
/// push notification option screen.
/// </summary>
public interface INotificationPreferencesRepository
{
    /// <summary>
    /// Returns active categories that have at least one active topic, each with its
    /// active topics loaded. Results are ordered by category display name.
    /// </summary>
    Task<IReadOnlyList<NotificationCategory>> GetCategoriesWithTopicsAsync(
        CancellationToken cancellationToken = default);
}
