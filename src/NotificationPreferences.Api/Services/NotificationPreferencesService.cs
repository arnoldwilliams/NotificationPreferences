using NotificationPreferences.Api.Repositories;
using NotificationPreferences.Contracts.Dtos;

namespace NotificationPreferences.Api.Services;

public class NotificationPreferencesService : INotificationPreferencesService
{
    private readonly INotificationPreferencesRepository _repository;

    public NotificationPreferencesService(INotificationPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationCategoryDto>> GetCategoryTopicsAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetCategoriesWithTopicsAsync(cancellationToken);

        return categories
            .Select(category => new NotificationCategoryDto
            {
                Id = category.Id,
                Code = category.Code,
                DisplayName = category.DisplayName,
                Topics = category.Topics
                    .OrderBy(topic => topic.DisplayName)
                    .Select(topic => new NotificationTopicDto
                    {
                        Id = topic.Id,
                        Code = topic.Code,
                        DisplayName = topic.DisplayName,
                        Description = topic.Description,
                        CategoryId = topic.CategoryId,
                    })
                    .ToList(),
            })
            .ToList();
    }
}