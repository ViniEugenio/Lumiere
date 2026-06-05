using Lumiere.Domain.Entities;
using Lumiere.Domain.Interfaces;
using Lumiere.Infra.Context;

namespace Lumiere.Infra.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }
}
