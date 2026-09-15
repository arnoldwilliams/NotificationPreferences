using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Api.Data.Configurations;

public class NotificationCategoryConfiguration : IEntityTypeConfiguration<NotificationCategory>
{
    public void Configure(EntityTypeBuilder<NotificationCategory> builder)
    {
        builder.ToTable("NotificationCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        // Code is VARCHAR(50): ASCII-only, so IsUnicode(false) avoids NVARCHAR.
        builder.Property(c => c.Code)
            .HasColumnType("varchar(50)")
            .IsUnicode(false)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.DisplayName)
            .HasColumnType("nvarchar(100)")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedUtc)
            .HasColumnType("datetime2(7)")
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("UQ_NotificationCategories_Code");
    }
}
