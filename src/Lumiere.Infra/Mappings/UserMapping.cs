using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Mappings;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(user => user.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(user => user.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(user => user.Active)
            .IsRequired()
            .HasColumnType("bit")
            .HasDefaultValue(true);

        builder.HasMany(user => user.Channels)
            .WithOne(channel => channel.User)
            .HasForeignKey(channel => channel.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
