## Why

`Lumiere.Domain.Entities.User` inherits from `Microsoft.AspNetCore.Identity.IdentityUser<int>`, and `AppDbContext` inherits from `IdentityDbContext<User, IdentityRole<int>, int>`. This was adopted to speed up initial scaffolding (table creation, password hashing) but it violates the project's own Clean Architecture rule that Domain must not depend on any framework. Investigation confirmed the project uses exactly one Identity feature — `UserManager.CreateAsync` for password hashing and persistence — while paying for six unused tables (`AspNetRoles`, `AspNetRoleClaims`, `AspNetUserClaims`, `AspNetUserRoles`, `AspNetUserLogins`, `AspNetUserTokens`) and unused columns (`NormalizedUserName`, `NormalizedEmail`, `EmailConfirmed`, `SecurityStamp`, `ConcurrencyStamp`, `PhoneNumber`, `PhoneNumberConfirmed`, `TwoFactorEnabled`, `LockoutEnd`, `LockoutEnabled`, `AccessFailedCount`). No authentication scheme, roles, claims, or lockout logic exist anywhere in the codebase. Password complexity is already independently enforced by `CreateUserCommandValidator` (FluentValidation), making Identity's internal password validator fully redundant.

This change also folds in two related cleanups identified during the same investigation: a duplicated entity convention (`CreatedAt`/`UpdatedAt`/`Active` plus identical `Activate()`/`Deactivate()` re-implemented in every entity) and a user-registration field set that needs to change from `Username`/`Email`/`Password`/`ConfirmPassword` to `FirstName`/`LastName`/`Email`/`ConfirmEmail`/`Password`/`ConfirmPassword`.

## What Changes

- **BREAKING**: Remove ASP.NET Core Identity entirely from the solution — no package reference, no `IdentityUser`, `IdentityDbContext`, `IdentityRole`, or `UserManager` anywhere in Domain, Application, or Infra.
- **BREAKING**: `User` entity stops inheriting `IdentityUser<int>`; becomes a plain POCO with `FirstName`, `LastName`, `Email`, `PasswordHash`.
- **BREAKING**: `UserName` column/property removed entirely — `Email` becomes the unique business identifier.
- **BREAKING**: `POST /api/users` request body changes from `{ Username, Email, Password, ConfirmPassword }` to `{ FirstName, LastName, Email, ConfirmEmail, Password, ConfirmPassword }`.
- Introduce an abstract `BaseEntity` (in `Lumiere.Domain/Common`) carrying `CreatedAt`, `UpdatedAt`, `Active`, plus the shared `Activate()`/`Deactivate()` behavior; `User` and `Channel` inherit from it, removing duplicated members.
- `IUserRepository.CreateUserAsync` changes signature to return `Task` (no result payload) — it only persists; uniqueness/error reporting stays entirely in the Application layer, which already performs that check before calling the repository.
- Password hashing moves to Infra using a project-owned implementation (no Identity package) — e.g. PBKDF2 via `System.Security.Cryptography`, fully within the BCL.
- New `UserMapping : IEntityTypeConfiguration<User>` following the existing `ChannelMapping`/old `UserMapping` pattern, adding a unique index on `Email`.
- `AppDbContext` becomes a plain `DbContext` (drops `IdentityDbContext<User, IdentityRole<int>, int>`), keeping only `Users` and `Channels` `DbSet`s.
- All existing migrations are deleted and replaced by a single fresh `InitialCreate` migration reflecting the new schema; the Development database is dropped and recreated via `dotnet ef database update` (`ASPNETCORE_ENVIRONMENT=Development`).
- `CLAUDE.md` updated: remove the "Nota histórica" about pending Identity removal (now resolved), document the `BaseEntity` convention (new entities must justify whether they inherit it), and document that auth fields are Infra-computed primitives on the entity.

## Capabilities

### New Capabilities
- `user-registration`: Defines the fields, validation, and uniqueness rules for creating a new user (replaces the implicit behavior that existed only as code, never documented as a spec).

### Modified Capabilities
(none — `user-registration` is being captured as a spec for the first time, not modifying a pre-existing spec)

## Impact

- **Domain**: `Entities/User.cs` (rewritten), `Entities/Channel.cs` (inherits `BaseEntity`), new `Common/BaseEntity.cs`, `Interfaces/IUserRepository.cs` (signature change)
- **Application**: `Features/Users/Commands/CreateUserCommand.cs`, `Features/Users/Handlers/CreateUserCommandHandler.cs`, `Validators/CreateUserCommandValidator.cs`
- **Infra**: `Context/AppDbContext.cs`, `Repositories/UserRepository.cs`, `Mappings/UserMapping.cs` (new), `DependencyInjection/IdentityExtensions.cs` (removed), all files under `Migrations/` (deleted and regenerated), `.csproj` (remove `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package reference)
- **API**: `POST /api/users` request contract changes (breaking)
- **Database**: Development database dropped and recreated with the new schema (destructive — confirmed acceptable since this is a dev-only database with no production data)
- **Docs**: `CLAUDE.md` (Entidades do Domínio section, new BaseEntity convention)
