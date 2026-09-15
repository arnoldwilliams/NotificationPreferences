using Microsoft.EntityFrameworkCore;
using NotificationPreferences.Api.Data;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Tests;

/// <summary>
/// Builds a real <see cref="NotificationPreferencesDbContext"/> over the in-memory provider so
/// the repository LINQ and the service mapping run as production code, with only the storage
/// backend swapped out.
/// </summary>
internal sealed class TestDbContextFactory : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public NotificationPreferencesDbContext CreateContext() => CreateContext(_databaseName);

    public NotificationPreferencesDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<NotificationPreferencesDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new NotificationPreferencesDbContext(options);
    }

    public NotificationPreferencesDbContext CreateSeededContext(
        IEnumerable<NotificationCategory> categories)
    {
        var context = CreateContext();
        context.NotificationCategories.AddRange(categories);
        context.SaveChanges();
        return context;
    }

    public void Dispose()
    {
        using var context = CreateContext();
        context.Database.EnsureDeleted();
    }
}
