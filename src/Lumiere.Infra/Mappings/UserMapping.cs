using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Mappings;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(u => u.Active)
            .IsRequired()
            .HasColumnType("bit")
            .HasDefaultValue(true);

        builder.HasMany(u => u.Canais)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
