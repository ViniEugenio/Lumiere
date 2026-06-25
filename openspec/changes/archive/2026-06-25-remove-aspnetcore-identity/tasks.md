## 1. BaseEntity extraction

- [x] 1.1 Create `Lumiere.Domain/Common/BaseEntity.cs` — abstract class with `CreatedAt` (`DateTime`), `UpdatedAt` (`DateTime?`), `Active` (`bool`), protected setters, and `Activate()`/`Deactivate()` methods
- [x] 1.2 Update `Channel` entity to inherit `BaseEntity`; remove its now-duplicated `CreatedAt`/`UpdatedAt`/`Active`/`Activate()`/`Deactivate()` members

## 2. Domain: User entity rewrite

- [x] 2.1 Rewrite `User` entity: remove `IdentityUser<int>` inheritance, inherit `BaseEntity` instead
- [x] 2.2 Add `FirstName`, `LastName`, `Email`, `PasswordHash` properties (private/protected setters as appropriate)
- [x] 2.3 Remove `UserName` property entirely
- [x] 2.4 Update `User.Create(...)` factory to accept `firstName`, `lastName`, `email` (no password — password set separately via a `SetPassword(string passwordHash)` method called from Infra)
- [x] 2.5 Update `IUserRepository.CreateUserAsync` signature to return `Task` (drop `Result<List<string>>`)

## 3. Infra: drop Identity, custom password hashing, new mapping

- [x] 3.1 Remove `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (and any other Identity package) from `Lumiere.Infra.csproj` — also removed from `Lumiere.API.csproj`, which had its own unused reference
- [x] 3.2 Delete `Lumiere.Infra/DependencyInjection/IdentityExtensions.cs` and remove its call site from `InfrastructureExtensions`/`AddInfrastructure`
- [x] 3.3 Rewrite `AppDbContext` as a plain `DbContext` (drop `IdentityDbContext<User, IdentityRole<int>, int>`), keep `Users` and `Channels` `DbSet`s
- [x] 3.4 Implement password hashing in `UserRepository` using `System.Security.Cryptography.Rfc2898DeriveBytes` (PBKDF2-HMACSHA256, random per-user salt)
- [x] 3.5 Rewrite `UserRepository.CreateUserAsync`: remove `UserManager<User>` dependency, hash password, call `user.SetPassword(hash)`, `context.Users.Add(user)`, `SaveChangesAsync`, return `Task`
- [x] 3.6 Create `Lumiere.Infra/Mappings/UserMapping.cs` (`IEntityTypeConfiguration<User>`) following the `ChannelMapping` pattern: table name, `CreatedAt`/`UpdatedAt`/`Active` column config (inherited from `BaseEntity`), unique index on `Email`

## 4. Application: registration fields and handler

- [x] 4.1 Update `CreateUserCommand`: fields become `FirstName`, `LastName`, `Email`, `ConfirmEmail`, `Password`, `ConfirmPassword`
- [x] 4.2 Update `CreateUserCommandValidator`: validate `FirstName`/`LastName` (required), `Email` (required, valid format), `ConfirmEmail` equals `Email`, keep existing `Password` complexity rules, `ConfirmPassword` equals `Password`
- [x] 4.3 Update `CreateUserCommandHandler`: validate `Email` uniqueness only (drop `Username` check), call `User.Create(firstName, lastName, email)`, call `userRepository.CreateUserAsync(user, password, ct)` without expecting a `Result` back — also removed the now-unused `UsernameAlreadyInUse` resource entry, and fixed `Lumiere.Tests/Setup/DatabaseSeed.cs` (referenced the old Identity-shaped `User`, not previously listed as a task but required to keep the build green)

## 5. Migrations: reset to a fresh InitialCreate

- [x] 5.1 Drop the Development database (`dotnet ef database drop` with `ASPNETCORE_ENVIRONMENT=Development`, or equivalent) — not needed: the configured `Server=lumiere` host didn't resolve on this machine (no existing DB to drop there); user updated `appsettings.Development.json` to `Server=localhost`, where the `Lumiere` database didn't exist yet
- [x] 5.2 Delete all existing files under `Lumiere.Infra/Migrations/` (including `AppDbContextModelSnapshot.cs`)
- [x] 5.3 Generate a fresh migration named `InitialCreate` reflecting the new schema (`Users` with `FirstName`, `LastName`, `Email` (unique), `PasswordHash`, `CreatedAt`, `UpdatedAt`, `Active`; `Channels` unchanged; no `AspNet*` tables) — verified via `INFORMATION_SCHEMA`
- [x] 5.4 Apply it with `dotnet ef database update`, `ASPNETCORE_ENVIRONMENT=Development` — confirmed only `Users`, `Channels`, `__EFMigrationsHistory` exist, no `AspNet*` tables

## 6. Documentation

- [x] 6.1 Update `CLAUDE.md` "Entidades do Domínio" section: remove the "Nota histórica" about pending Identity removal (now resolved); state `User` is a plain entity inheriting `BaseEntity`
- [x] 6.2 Add a `BaseEntity` convention to `CLAUDE.md`: new entities must confirm whether they should inherit `Lumiere.Domain.Common.BaseEntity` for `CreatedAt`/`UpdatedAt`/`Active`/`Activate()`/`Deactivate()`, documenting any justified exception
- [x] 6.3 Note in `CLAUDE.md` that `PasswordHash` is an Infra-computed primitive on the entity, with no Identity package involved anywhere in the solution

## 7. Final verification

- [x] 7.1 Build the solution; confirm zero references to `Microsoft.AspNetCore.Identity*` anywhere (`grep`/search across all `.csproj` and `.cs` files) — `dotnet build` succeeded, 0 warnings/errors; no `.csproj` references Identity anywhere
- [x] 7.2 Run the test suite (currently empty, but confirm it still builds/runs) — runs cleanly, no tests present
- [x] 7.3 Manually verify `POST /api/users` against the recreated Development database: valid payload succeeds (200) ✓; duplicate `Email` rejected (400) ✓. **Mismatched `ConfirmEmail`/`ConfirmPassword` incorrectly returned 200 instead of 400** — root cause identified: FluentValidation validators are registered in DI but no `IPipelineBehavior` wires them into the MediatR pipeline (`Behaviors/` folder is empty, `MediatrExtensions.cs` has no validation behavior). This is a **pre-existing gap affecting the entire Application layer**, not introduced by this change — user explicitly decided not to fix it as part of this change (option 2: leave as known gap, address separately)
