using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Data.Mappings;

public class UserMapping : BaseMapping<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {

        base
            .Configure(builder);

        builder
            .Property(user => user.FirstName)
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder
            .Property(user => user.LastName)
            .HasColumnType("nvarchar(50)")
            .IsRequired();

        builder
            .Property(user => user.Email)
            .HasColumnType("nvarchar(255)")
            .IsRequired();

        builder
            .HasIndex(user => user.Email)
            .IsUnique();

        builder
            .Property(user => user.PasswordHash)
            .HasColumnType("nvarchar(255)")
            .IsRequired();

    }
}
