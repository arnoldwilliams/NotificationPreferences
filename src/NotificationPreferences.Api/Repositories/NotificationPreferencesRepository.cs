using Microsoft.EntityFrameworkCore;
using NotificationPreferences.Api.Data;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Api.Repositories;

public class NotificationPreferencesRepository : INotificationPreferencesRepository
{
    private readonly NotificationPreferencesDbContext _dbContext;

    public NotificationPreferencesRepository(NotificationPreferencesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<NotificationCategory>> GetCategoriesWithTopicsAsync(
        CancellationToken cancellationToken = default)
    {
        // Single query: EF Core splits the filtered include so the topics are restricted
        // to IsActive on the database side rather than being filtered in memory.
        return await _dbContext.NotificationCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Topics.Where(t => t.IsActive))
            .Where(c => c.Topics.Any(t => t.IsActive))
            .OrderBy(c => c.DisplayName)
            .ToListAsync(cancellationToken);
    }
}