## Context

`Lumiere.Application` currently has two Application Services: `UserService` (used only by `CreateUserCommandHandler`) and `DataBaseService` (used only by `UpdateDataBaseHandler`). Each is a 1:1 wrapper — no other handler calls them, so they add a layer of indirection without behavioral benefit. The discussion that led to this change concluded that, per CQRS and Clean Architecture theory, a `Handler` already plays the role of the Use Case/Interactor and may call a `Repository` (Gateway) directly. A Service is only justified once two or more handlers need the same orchestration logic — extracted on actual duplication, not pre-emptively.

## Goals / Non-Goals

**Goals:**
- Remove `UserService`/`IUserService` and `DataBaseService`/`IDataBaseService`, moving their logic into the handlers that are their only callers.
- Update `CLAUDE.md` to document the revised rule: handlers may call repositories directly when they are the sole consumer of that orchestration; a Service is introduced only when a second handler needs the same logic.
- Preserve current external behavior exactly (API responses, validation errors, persistence side effects) — this is a structural refactor, not a behavior change.

**Non-Goals:**
- No change to Domain entities, DTOs, Commands, or API endpoints.
- No new validation rules or business logic.
- Not addressing other Application-layer concerns (e.g. pipeline behaviors, FluentValidation) outside of this Service-removal scope.
- Not establishing a blanket "no Services ever" rule — Services remain valid once real reuse across handlers exists.

## Decisions

**Decision: Inline `UserService.CreateUserAsync` + `ValidateCreateUserAsync` into `CreateUserCommandHandler`.**
- The handler will take a primary constructor dependency on `IUserRepository` instead of `IUserService`.
- The private validation helper moves into the handler as a private method, keeping the existing public/private member ordering convention (constructor → public `Handle` → private helpers).
- Alternative considered: keep `UserService` "just in case" of future reuse. Rejected — speculative reuse is exactly the premature abstraction this change removes; if a second handler needs this logic later, extracting it back into a Service at that point is cheap and well-scoped to the actual duplication.

**Decision: Inline `DataBaseService.UpdateBase` into `UpdateDataBaseHandler`.**
- Same pattern: handler depends on `IDataBaseRepository` directly, try/catch and `ResultDto<object>` construction move into `Handle`.

**Decision: Remove DI registrations in `ServicesExtensions.AddServices()`.**
- Both registrations (`IUserService`, `IDataBaseService`) are deleted. If no services remain afterward, evaluate whether `AddServices()` and its call site should be removed too, or left as a no-op extension point for future Services.

**Decision: Update `CLAUDE.md` Application layer section.**
- Replace the absolute rule "CommandHandlers não devem acessar repositórios diretamente" with guidance reflecting the reuse-based criterion: a handler may depend on a repository directly when it is the only consumer of that orchestration; once a second handler needs the same logic, extract a Service.

## Risks / Trade-offs

- [Risk] Future handler duplicates validation logic before anyone notices the need for a Service → Mitigation: code review checklist / this convention documented in `CLAUDE.md` so reviewers know to flag duplication and extract at that point.
- [Risk] Existing unit tests (if any) target `UserService`/`DataBaseService` directly and will break → Mitigation: move/rewrite those tests to target the handlers; covered in tasks.md.
- [Risk] Mixing this with other in-flight Application-layer changes could cause merge conflicts → Mitigation: scope this change strictly to the two services and their handlers, no unrelated refactoring.

## Migration Plan

1. Inline `DataBaseService` logic into `UpdateDataBaseHandler`; delete `DataBaseService`/`IDataBaseService`.
2. Inline `UserService` logic into `CreateUserCommandHandler`; delete `UserService`/`IUserService`.
3. Remove the now-unused DI registrations from `ServicesExtensions`.
4. Update/move any existing tests referencing the removed services.
5. Update `CLAUDE.md`.
6. Build and run the test suite to confirm no regression.

No rollback complexity: this is a pure code-structure change with no data migration or external contract change. Rollback is a straight `git revert` if needed.

## Open Questions

- None — scope is fully determined by the two existing services and their single callers.
