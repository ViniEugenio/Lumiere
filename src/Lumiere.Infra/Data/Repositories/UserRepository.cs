using Lumiere.Application.Interfaces.Repositories;
using Lumiere.Domain.Entities;

namespace Lumiere.Infra.Data.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
}
