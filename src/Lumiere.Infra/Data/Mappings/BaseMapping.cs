using Lumiere.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lumiere.Infra.Data.Mappings
{
    public class BaseMapping<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {

            builder
                .HasKey(entity => entity.Id);

            builder
                .Property(entity => entity.CreatedAt)
                .HasColumnType("smalldatetime")
                .IsRequired();

            builder
                .Property(entity => entity.UpdatedAt)
                .HasColumnType("smalldatetime");

            builder
                .Property(entity => entity.Active)
                .HasColumnType("bit")
                .IsRequired();

        }
    }
}
