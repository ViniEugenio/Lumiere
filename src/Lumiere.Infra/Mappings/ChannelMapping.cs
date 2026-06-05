using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Mappings;

public class ChannelMapping : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("Channels");

        builder.HasKey(channel => channel.Id);
        builder.Property(channel => channel.Id).ValueGeneratedOnAdd();

        builder.Property(channel => channel.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(channel => channel.Description)
            .HasMaxLength(250);

        builder.Property(channel => channel.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(channel => channel.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(channel => channel.Active)
            .IsRequired()
            .HasColumnType("bit")
            .HasDefaultValue(true);
    }
}
