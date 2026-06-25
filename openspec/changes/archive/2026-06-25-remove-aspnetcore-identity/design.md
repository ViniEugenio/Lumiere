## Context

`User : IdentityUser<int>` and `AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>` were adopted early for scaffolding speed. Investigation (see proposal) confirmed only `UserManager.CreateAsync` (hash + persist) is actually used; roles, claims, lockout, 2FA, phone, and email-confirmation are dead columns/tables. Password complexity is already independently enforced by `CreateUserCommandValidator`. This is a dev-only database (`InitialCreate` migration from 2026-06-05, no production data), so a destructive schema reset is acceptable.

Two related cleanups are folded in because they touch the same files: extracting a `BaseEntity` (removing duplicated `CreatedAt`/`UpdatedAt`/`Active`/`Activate()`/`Deactivate()` across `User` and `Channel`), and changing the registration field set from `Username`/`Email`/`Password`/`ConfirmPassword` to `FirstName`/`LastName`/`Email`/`ConfirmEmail`/`Password`/`ConfirmPassword`.

## Goals / Non-Goals

**Goals:**
- Zero references to any `Microsoft.AspNetCore.Identity*` type or package anywhere in the solution (Domain, Application, Infra, API).
- `User` becomes a plain Domain POCO: `FirstName`, `LastName`, `Email`, `PasswordHash`, inheriting `BaseEntity`.
- `BaseEntity` (abstract, in `Lumiere.Domain/Common`) owns `CreatedAt`, `UpdatedAt`, `Active`, `Activate()`, `Deactivate()`; `User` and `Channel` inherit it.
- `IUserRepository.CreateUserAsync` returns `Task` — persistence only, no result payload.
- A single fresh `InitialCreate` migration replaces all existing migrations, applied to the Development database.
- `CLAUDE.md` reflects the new BaseEntity convention and resolves the Identity "Nota histórica".

**Non-Goals:**
- Implementing login/authentication (JWT, cookies, sessions) — this change only touches registration.
- Implementing a real email-confirmation flow (sending a verification link/token). `ConfirmEmail` here is a request-only field validated for equality against `Email`, exactly like `ConfirmPassword` against `Password` today — not persisted, not a verification token.
- Solving distributed-transaction-level race conditions beyond what a database unique constraint already prevents.

## Decisions

**Decision: Replace Identity's password hashing with a BCL-only implementation (PBKDF2 via `System.Security.Cryptography.Rfc2898DeriveBytes`), living entirely in Infra.**
- `IUserRepository.CreateUserAsync(User user, string password, CancellationToken)` keeps its current signature shape (raw password in, Infra hashes it) — only the *implementation* changes, not the contract Application already calls.
- Infra computes a salted hash and calls a new entity method (e.g. `user.SetPassword(hash)`, mirroring the existing `Update()`/`Activate()` mutation-method pattern with private setters) before `context.Users.Add(user)` + `SaveChangesAsync`.
- Alternative considered: keep using `PasswordHasher<T>` from `Microsoft.Extensions.Identity.Core` as a narrowly-scoped utility (no `IdentityDbContext`, no `UserManager`). Rejected — it still pulls an `Microsoft.AspNetCore.Identity*` package into the solution, which contradicts the explicit goal of zero Identity installations across all layers.
- Alternative considered: third-party library (BCrypt.Net). Rejected — BCL already provides PBKDF2 via `Rfc2898DeriveBytes`, no need for an extra dependency for this scope.

**Decision: `BaseEntity` is abstract, holds only `CreatedAt`, `UpdatedAt`, `Active` (not `Id`) plus `Activate()`/`Deactivate()`.**
- `Id` stays declared per-entity (matches the existing global convention statement "Todas as chaves primárias são do tipo `int`" without bundling it into a shared base — keeps `BaseEntity` focused on lifecycle, not identity).
- Properties use `protected set` so derived `Create()` factory methods can assign `CreatedAt`/`Active`, while external callers can't mutate them directly.
- `Activate()`/`Deactivate()` move to `BaseEntity` verbatim (current implementations in `User` and `Channel` are byte-for-byte identical), removing duplication.

**Decision: `IUserRepository.CreateUserAsync` returns `Task`, not `Task<Result<List<string>>>`.**
- Uniqueness validation already happens in `CreateUserCommandHandler` before calling the repository (existing `ExistsAsync` check, now checking only `Email`). The repository's job per the project's own rule ("Repositórios são responsáveis apenas pela persistência — não convertem erros em DTOs") is just to persist; returning a `Result` for the happy path added no value, since the only failure path through Identity's internal re-check is being removed entirely.

**Decision: Single fresh `InitialCreate` migration, dev database dropped and recreated.**
- Deleting migration files alone does not retroactively change what's already applied to the dev database; without a drop, the database keeps `AspNetRoles`/etc. with no migration history pointing back to them, corrupting the migration story.
- Sequence: drop dev database → delete `Migrations/` folder contents → `dotnet ef migrations add InitialCreate` → `dotnet ef database update` with `ASPNETCORE_ENVIRONMENT=Development`.

**Decision: Unique index on `Email` at the database level, in `UserMapping`.**
- The application-level `ExistsAsync` check remains (fast-fail with a clean validation error in the common case), but it cannot fully close the race window between check and insert. The unique index is the actual correctness guarantee; the app check is a UX optimization for the common case.

**Decision: `ConfirmEmail` and `ConfirmPassword` are request-only fields on `CreateUserCommand`, validated for equality, never persisted.**
- Mirrors the existing `ConfirmPassword` pattern already in `CreateUserCommandValidator` — no new validation idiom introduced.

## Risks / Trade-offs

- [Risk] Race condition: two concurrent requests with the same email could both pass the app-level `ExistsAsync` check before either commits → [Mitigation] unique DB index guarantees only one row ever persists; the loser gets an unhandled `DbUpdateException` (500) instead of a clean validation 400. Accepted trade-off — fixing it cleanly would require the repository to catch and translate the constraint-violation exception, which conflicts with the project rule that repositories never convert errors into DTOs. This is a pre-existing gap (same race existed before this change, just surfaced differently), not introduced by this refactor.
- [Risk] Hand-rolled password hashing done incorrectly is a security risk → [Mitigation] use `Rfc2898DeriveBytes` (PBKDF2-HMACSHA256) from the BCL with a random salt per user and a standard iteration count — well-established primitive, not a custom cipher.
- [Risk] Dropping the Development database is destructive → [Mitigation] confirmed acceptable — dev-only data, explicit request, no production environment affected.
- [Risk] Removing `UserName` could silently break something that reads it → [Mitigation] confirmed via codebase search that nothing outside the entity/Identity plumbing references `UserName`; `FirstName`+`LastName` become the display identity going forward.

## Migration Plan

1. Remove `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (and any other Identity package) from `Lumiere.Infra.csproj`; confirm no other project references an Identity package.
2. Rewrite `Lumiere.Domain.Common.BaseEntity` and update `User`/`Channel` to inherit it.
3. Rewrite `User` entity: remove `IdentityUser<int>` inheritance, add `FirstName`, `LastName`, `PasswordHash`, remove `UserName`.
4. Update `IUserRepository.CreateUserAsync` signature to return `Task`.
5. Rewrite `UserRepository`: remove `UserManager<User>` dependency, add BCL-based password hashing, direct `context.Users.Add` + `SaveChangesAsync`.
6. Add `UserMapping : IEntityTypeConfiguration<User>` with unique index on `Email`.
7. Rewrite `AppDbContext` as plain `DbContext` (drop `IdentityDbContext`/`IdentityRole`).
8. Remove `IdentityExtensions.cs` and its call site in `InfrastructureExtensions`/`AddInfrastructure`.
9. Update `CreateUserCommand` fields and `CreateUserCommandValidator` rules.
10. Update `CreateUserCommandHandler` for the new fields and the new repository signature.
11. Drop the Development database; delete all files under `Migrations/`; generate fresh `InitialCreate`; run `dotnet ef database update` with `ASPNETCORE_ENVIRONMENT=Development`.
12. Update `CLAUDE.md`.
13. Build, and manually verify `POST /api/users` end to end against the recreated Development database.

No rollback beyond `git revert` — there is no production data to preserve.

## Open Questions

None outstanding — scope and approach were confirmed directly with the project owner during exploration.
