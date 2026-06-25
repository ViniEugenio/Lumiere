using Bogus;
using Lumiere.Domain.Entities;
using Lumiere.Infra.Context;

namespace Lumiere.Tests.Setup;

public static class DatabaseSeed
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var users = GenerateUsers(5);
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var channels = GenerateChannels(users);
        await context.Channels.AddRangeAsync(channels);
        await context.SaveChangesAsync();
    }

    private static List<User> GenerateUsers(int count)
    {
        return new Faker<User>("pt_BR")
            .CustomInstantiator(faker =>
                User.Create(
                    faker.Name.FirstName(),
                    faker.Name.LastName(),
                    faker.Internet.Email()
                ))
            .Generate(count)
            .Select(user =>
            {
                user.SetPassword("seeded-password-hash");
                return user;
            })
            .ToList();
    }

    private static List<Channel> GenerateChannels(List<User> users)
    {
        return new Faker<Channel>("pt_BR")
            .CustomInstantiator(faker =>
                Channel.Create(
                    faker.Lorem.Word(),
                    faker.Lorem.Sentence(10)[..Math.Min(250, faker.Lorem.Sentence(10).Length)],
                    faker.PickRandom(users).Id
                ))
            .Generate(users.Count * 2);
    }
}
