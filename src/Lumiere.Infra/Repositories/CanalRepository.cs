using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;

namespace Lumiere.Infra.Repositories;

public class CanalRepository : BaseRepository<Canal>, ICanalRepository
{
    public CanalRepository(AppDbContext context) : base(context) { }
}
