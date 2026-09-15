using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.Api.Services;

public interface INotificationPreferencesService
{
    /// <summary>
    /// Builds the grouped category/topic list consumed by the MAUI push notification
    /// options screen. Only active categories containing at least one active topic are returned.
    /// </summary>
    Task<IReadOnlyList<NotificationCategoryDto>> GetCategoryTopicsAsync(
        CancellationToken cancellationToken = default);
}