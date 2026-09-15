using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationPreferences.Api.Data.Entities;

namespace NotificationPreferences.Api.Data.Configurations;

public class NotificationTopicConfiguration : IEntityTypeConfiguration<NotificationTopic>
{
    public void Configure(EntityTypeBuilder<NotificationTopic> builder)
    {
        builder.ToTable("NotificationTopics");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.CategoryId)
            .IsRequired();

        builder.Property(t => t.Code)
            .HasColumnType("varchar(100)")
            .IsUnicode(false)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.DisplayName)
            .HasColumnType("nvarchar(150)")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnType("nvarchar(250)")
            .HasMaxLength(250);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(t => t.CreatedUtc)
            .HasColumnType("datetime2(7)")
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Topics)
            .HasForeignKey(t => t.CategoryId)
            .HasConstraintName("FK_NotificationTopics_NotificationCategories_CategoryId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => new { t.CategoryId, t.Code })
            .IsUnique()
            .HasDatabaseName("UQ_NotificationTopics_CategoryId_Code");
    }
}
