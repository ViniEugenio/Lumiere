using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Mappings;

public class CanalMapping : IEntityTypeConfiguration<Canal>
{
    public void Configure(EntityTypeBuilder<Canal> builder)
    {
        builder.ToTable("Canais");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasMaxLength(250);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(c => c.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(c => c.Active)
            .IsRequired()
            .HasColumnType("bit")
            .HasDefaultValue(true);
    }
}
