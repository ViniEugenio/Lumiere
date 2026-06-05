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

        var canais = GenerateCanais(users);
        await context.Canais.AddRangeAsync(canais);
        await context.SaveChangesAsync();
    }

    private static List<User> GenerateUsers(int count)
    {
        return new Faker<User>("pt_BR")
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.NormalizedEmail, (_, u) => u.Email!.ToUpperInvariant())
            .RuleFor(u => u.NormalizedUserName, (_, u) => u.UserName!.ToUpperInvariant())
            .RuleFor(u => u.PasswordHash, _ => new PasswordHasher<User>().HashPassword(null!, "Test@1234"))
            .RuleFor(u => u.SecurityStamp, _ => Guid.NewGuid().ToString())
            .RuleFor(u => u.CreatedAt, f => f.Date.Past(1))
            .RuleFor(u => u.Active, _ => true)
            .Generate(count);
    }

    private static List<Canal> GenerateCanais(List<User> users)
    {
        return new Faker<Canal>("pt_BR")
            .CustomInstantiator(f =>
                Canal.Create(
                    f.Lorem.Word(),
                    f.Lorem.Sentence(10)[..Math.Min(250, f.Lorem.Sentence(10).Length)],
                    f.PickRandom(users).Id
                ))
            .Generate(users.Count * 2);
    }
}
