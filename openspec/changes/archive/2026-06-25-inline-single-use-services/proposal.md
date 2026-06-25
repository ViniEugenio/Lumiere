## Why

`UserService` and `DataBaseService` are each called from exactly one `CommandHandler`, with no reuse across handlers. Per CQRS/Clean Architecture, the Handler already plays the role of the Use Case/Interactor — a Service layer only earns its place when its orchestration logic is shared by multiple handlers. Today these two services are pure pass-through indirection: extra files, an extra DI registration, and an extra hop with no behavioral benefit.

## What Changes

- **BREAKING** (internal only, no public API change): Remove `IUserService` / `UserService` and `IDataBaseService` / `DataBaseService` along with their DI registrations.
- Move `UserService.CreateUserAsync` (and its private `ValidateCreateUserAsync` helper) directly into `CreateUserCommandHandler`.
- Move `DataBaseService.UpdateBase` directly into `UpdateDataBaseHandler`.
- Handlers call `IUserRepository` / `IDataBaseRepository` directly, which is now allowed for handlers that own their full use case with no cross-handler reuse.
- Update `CLAUDE.md` Application layer rules to reflect the new guidance: Handlers may access repositories directly when no other handler needs the same orchestration; a Service is introduced only once a second handler needs the same logic (extract on actual duplication, not preemptively).

## Capabilities

### New Capabilities

(none — this is an internal refactor, no new behavior)

### Modified Capabilities

(none — no spec-level/requirement behavior changes; user-facing behavior of user creation and database migration stays identical)

## Impact

- **Code removed**: `Lumiere.Application/Services/Interfaces/IUserService.cs`, `Services/Implementations/UserService.cs`, `Services/Interfaces/IDataBaseService.cs`, `Services/Implementations/DataBaseService.cs`
- **Code changed**: `Features/Users/Handlers/CreateUserCommandHandler.cs`, `Features/Database/Handlers/UpdateDataBaseHandler.cs`, `DependencyInjection/ServicesExtensions.cs` (drop service registrations; if no services remain, evaluate removing `AddServices()` call)
- **Docs changed**: `CLAUDE.md` (Application layer section — Services responsibility rule)
- **Tests affected**: any existing unit tests targeting `UserService`/`DataBaseService` directly must move to target the handlers instead
- No changes to API endpoints, DTOs, Commands, or Domain entities
