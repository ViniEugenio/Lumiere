using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Data.Mappings
{
    public class RoleMapping : BaseMapping<Role>
    {

        public override void Configure(EntityTypeBuilder<Role> builder)
        {

            base.Configure(builder);

            builder
                .Property(role => role.Name)
                .HasColumnType("varchar(50)")
                .IsRequired();

        }

    }
}
