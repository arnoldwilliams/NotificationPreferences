using System.Net.Http.Json;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.Client.Services;

public interface INotificationPreferencesApiClient
{
    Task<IReadOnlyList<NotificationCategoryDto>> GetCategoriesWithTopicsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Fetches the notification option groups from the NotificationPreferences API.
/// </summary>
public sealed class NotificationPreferencesApiClient : INotificationPreferencesApiClient
{
    private const string CategoriesWithTopicsPath = "api/notification-preferences/categories-with-topics";

    private readonly HttpClient _httpClient;

    public NotificationPreferencesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<NotificationCategoryDto>> GetCategoriesWithTopicsAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(CategoriesWithTopicsPath, cancellationToken);
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<NotificationCategoryDto>>(
            cancellationToken: cancellationToken);

        return categories ?? [];
    }
}
