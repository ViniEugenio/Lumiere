using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Data.Mappings
{
    public class UserRoleMapping : BaseMapping<UserRole>
    {

        public override void Configure(EntityTypeBuilder<UserRole> builder)
        {

            base.Configure(builder);

            builder
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.Roles)
                .HasForeignKey(userRole => userRole.UserId);

            builder
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId);

        }

    }
}
