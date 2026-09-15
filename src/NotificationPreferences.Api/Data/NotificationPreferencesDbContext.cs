using Microsoft.EntityFrameworkCore;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Api.Data;

public class NotificationPreferencesDbContext : DbContext
{
    public NotificationPreferencesDbContext(DbContextOptions<NotificationPreferencesDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationCategory> NotificationCategories => Set<NotificationCategory>();

    public DbSet<NotificationTopic> NotificationTopics => Set<NotificationTopic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationPreferencesDbContext).Assembly);
    }
}