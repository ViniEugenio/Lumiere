using Bogus;
using Lumiere.Domain.Entities;
using Lumiere.Infra.Context;
using Microsoft.AspNetCore.Identity;

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
            .RuleFor(user => user.UserName, faker => faker.Internet.UserName())
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.NormalizedEmail, (_, user) => user.Email!.ToUpperInvariant())
            .RuleFor(user => user.NormalizedUserName, (_, user) => user.UserName!.ToUpperInvariant())
            .RuleFor(user => user.PasswordHash, _ => new PasswordHasher<User>().HashPassword(null!, "Test@1234"))
            .RuleFor(user => user.SecurityStamp, _ => Guid.NewGuid().ToString())
            .RuleFor(user => user.CreatedAt, faker => faker.Date.Past(1))
            .RuleFor(user => user.Active, _ => true)
            .Generate(count);
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
