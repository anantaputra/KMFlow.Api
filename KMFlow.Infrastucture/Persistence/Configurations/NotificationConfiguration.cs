using KMFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KMFlow.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.NotificationId);

        builder.Property(x => x.NotificationId)
            .HasColumnName("NotificationID")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("UserID")
            .IsRequired();

        builder.Property(x => x.KnowledgeId)
            .HasColumnName("KnowledgeID");

        builder.Property(x => x.Type)
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnType("varchar(255)")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.IsRead)
            .HasColumnType("bit")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.RelatedActionBy)
            .HasColumnName("RelatedActionBy");

        builder.Property(x => x.CreatedDate)
            .HasColumnType("datetime")
            .HasDefaultValueSql("GETDATE()")
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(x => x.Knowledge)
            .WithMany()
            .HasForeignKey(x => x.KnowledgeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.RelatedActionUser)
            .WithMany()
            .HasForeignKey(x => x.RelatedActionBy)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
