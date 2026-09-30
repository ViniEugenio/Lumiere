using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Domain.Entities;

namespace Lumiere.Infra.Data.Repositories;

public class ChannelRepository(AppDbContext context) : BaseRepository<Channel>(context), IChannelRepository;
