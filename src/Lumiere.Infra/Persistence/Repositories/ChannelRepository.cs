using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Data;

namespace Lumiere.Infra.Persistence.Repositories;

public class ChannelRepository(AppDbContext context) : BaseRepository<Channel>(context), IChannelRepository;
