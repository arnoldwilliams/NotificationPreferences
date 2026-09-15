using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NotificationPreferences.Api.Data;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Tests;

/// <summary>
/// Asserts the EF model matches the existing dbo.NotificationCategories / dbo.NotificationTopics
/// DDL. Uses the SQL Server provider so the relational facet (column types, index and constraint
/// names) is populated; no connection is opened because the model is built from configuration.
/// </summary>
public class NotificationPreferencesModelMappingTests
{
    private static readonly DbContextOptions<NotificationPreferencesDbContext> Options =
        new DbContextOptionsBuilder<NotificationPreferencesDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelMappingOnly;")
            .Options;

    private static NotificationPreferencesDbContext CreateContext() => new(Options);

    [Theory]
    [InlineData("NotificationCategories")]
    [InlineData("NotificationTopics")]
    public void Entities_MapToExpectedTables(string tableName)
    {
        using var context = CreateContext();

        var entityType = Assert.Single(context.Model.GetEntityTypes().Where(e => e.GetTableName() == tableName));

        Assert.Equal("dbo", entityType.GetSchema() ?? "dbo");
    }

    [Fact]
    public void CategoryCode_IsVarchar50_AndDisplayNameIsNvarchar100()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NotificationCategory))!;

        Assert.Equal("varchar(50)", entityType.FindProperty(nameof(NotificationCategory.Code))!.GetColumnType());
        Assert.Equal("nvarchar(100)", entityType.FindProperty(nameof(NotificationCategory.DisplayName))!.GetColumnType());
        Assert.Equal("bit", entityType.FindProperty(nameof(NotificationCategory.IsActive))!.GetColumnType());
        Assert.Equal("datetime2(7)", entityType.FindProperty(nameof(NotificationCategory.CreatedUtc))!.GetColumnType());
    }

    [Fact]
    public void TopicColumns_MatchDeclaredTypesAndNullability()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NotificationTopic))!;

        Assert.Equal("varchar(100)", entityType.FindProperty(nameof(NotificationTopic.Code))!.GetColumnType());
        Assert.Equal("nvarchar(150)", entityType.FindProperty(nameof(NotificationTopic.DisplayName))!.GetColumnType());
        Assert.Equal("nvarchar(250)", entityType.FindProperty(nameof(NotificationTopic.Description))!.GetColumnType());
        Assert.Equal("datetime2(7)", entityType.FindProperty(nameof(NotificationTopic.CreatedUtc))!.GetColumnType());

        Assert.True(entityType.FindProperty(nameof(NotificationTopic.Description))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(NotificationTopic.Code))!.IsNullable);
        Assert.False(entityType.FindProperty(nameof(NotificationTopic.CategoryId))!.IsNullable);
    }

    [Fact]
    public void IdentityColumns_AreGeneratedOnAdd()
    {
        using var context = CreateContext();

        var categoryId = context.Model.FindEntityType(typeof(NotificationCategory))!
            .FindProperty(nameof(NotificationCategory.Id))!;
        var topicId = context.Model.FindEntityType(typeof(NotificationTopic))!
            .FindProperty(nameof(NotificationTopic.Id))!;

        Assert.Equal(ValueGenerated.OnAdd, categoryId.ValueGenerated);
        Assert.Equal(ValueGenerated.OnAdd, topicId.ValueGenerated);
    }

    [Fact]
    public void UniqueIndexes_MatchDeclaredConstraintNames()
    {
        using var context = CreateContext();

        var categoryIndex = Assert.Single(
            context.Model.FindEntityType(typeof(NotificationCategory))!.GetIndexes(),
            i => i.GetDatabaseName() == "UQ_NotificationCategories_Code");

        var topicIndex = Assert.Single(
            context.Model.FindEntityType(typeof(NotificationTopic))!.GetIndexes(),
            i => i.GetDatabaseName() == "UQ_NotificationTopics_CategoryId_Code");

        Assert.True(categoryIndex.IsUnique);
        Assert.True(topicIndex.IsUnique);
        Assert.Equal(
            [nameof(NotificationTopic.CategoryId), nameof(NotificationTopic.Code)],
            topicIndex.Properties.Select(p => p.Name));
    }

    [Fact]
    public void TopicCategoryForeignKey_UsesDeclaredConstraintName()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(NotificationTopic))!;

        var foreignKey = Assert.Single(entityType.GetForeignKeys());

        Assert.Equal("FK_NotificationTopics_NotificationCategories_CategoryId", foreignKey.GetConstraintName());
        Assert.Equal(typeof(NotificationCategory), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void GeneratedDdl_MatchesHandWrittenSchema()
    {
        using var context = CreateContext();

        var script = context.Database.GenerateCreateScript();

        // Column types must match the shipped DDL. varchar vs nvarchar is the easy thing to get
        // wrong here, and the difference is silent until locale-specific data is stored.
        Assert.Contains("[Code] varchar(50) NOT NULL", script);
        Assert.Contains("[DisplayName] nvarchar(100) NOT NULL", script);
        Assert.Contains("[Code] varchar(100) NOT NULL", script);
        Assert.Contains("[DisplayName] nvarchar(150) NOT NULL", script);
        Assert.Contains("[Description] nvarchar(250) NULL", script);
        Assert.Contains("[CreatedUtc] datetime2(7) NOT NULL DEFAULT (SYSUTCDATETIME())", script);
        Assert.Contains("IDENTITY", script);

        // Constraint and index names are referenced by any future migration or script, so a drift
        // here would break deployments rather than the compile.
        Assert.Contains("CONSTRAINT [PK_NotificationCategories] PRIMARY KEY", script);
        Assert.Contains("CONSTRAINT [PK_NotificationTopics] PRIMARY KEY", script);
        Assert.Contains("CONSTRAINT [FK_NotificationTopics_NotificationCategories_CategoryId]", script);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_NotificationCategories_Code]", script);
        Assert.Contains("CREATE UNIQUE INDEX [UQ_NotificationTopics_CategoryId_Code]", script);
    }
}