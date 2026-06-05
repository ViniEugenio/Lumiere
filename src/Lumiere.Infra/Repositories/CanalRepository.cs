using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;

namespace Lumiere.Infra.Repositories;

public class CanalRepository(AppDbContext context) : BaseRepository<Canal>(context), ICanalRepository;
