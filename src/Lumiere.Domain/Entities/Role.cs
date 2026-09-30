namespace Lumiere.Domain.Entities
{
    public class Role : BaseEntity
    {

        public string Name { get; private set; }

        public List<UserRole> UserRoles { get; private set; } = [];

        public static Role Create(string name)
        {
            
            return new Role()
            {
                Name = name
            };

        }

    }
}
