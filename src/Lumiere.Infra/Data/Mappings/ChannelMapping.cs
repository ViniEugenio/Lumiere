using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Data.Mappings;

public class ChannelMapping : BaseMapping<Channel>
{
    public override void Configure(EntityTypeBuilder<Channel> builder)
    {

        base
            .Configure(builder);

        builder
            .Property(channel => channel.Name)
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder
            .Property(channel => channel.Description)
            .HasColumnType("nvarchar(4000)")
            .IsRequired();

        builder
            .HasOne(channel => channel.User)
            .WithMany(user => user.Channels)
            .HasForeignKey(channel => channel.UserId);

    }
}
