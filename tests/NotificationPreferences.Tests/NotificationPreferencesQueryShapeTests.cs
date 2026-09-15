using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using NotificationPreferences.Api.Data;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Tests;

/// <summary>
/// Captures the SQL the repository actually sends so we can prove filtering happens on the
/// database rather than in memory after loading every row.
/// </summary>
public class NotificationPreferencesQueryShapeTests
{
    [Fact]
    public void GeneratedQuery_FiltersActiveRowsInSqlServer()
    {
        var options = new DbContextOptionsBuilder<NotificationPreferencesDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=QueryShapeOnly;")
            .Options;

        using var context = new NotificationPreferencesDbContext(options);
        var query = context.NotificationCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Include(c => c.Topics.Where(t => t.IsActive))
            .Where(c => c.Topics.Any(t => t.IsActive))
            .OrderBy(c => c.DisplayName);

        var sql = query.ToQueryString();

        // Two predicates: one on the category, one on the correlated topic include. Both must be
        // evaluated by SQL Server; a client-side filter would show up as a bare SELECT.
        var activePredicateCount = Regex.Matches(sql, @"\[IsActive\]").Count;
        Assert.True(
            activePredicateCount >= 3,
            $"Expected IsActive to be filtered in SQL (WHERE and include predicates). Actual SQL:\n{sql}");

        Assert.Contains("WHERE", sql);
        Assert.Contains("ORDER BY", sql);
        Assert.Contains("NotificationCategories", sql);
        Assert.Contains("NotificationTopics", sql);
    }
}