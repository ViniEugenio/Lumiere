## 1. Database feature: inline `DataBaseService`

- [x] 1.1 Move `DataBaseService.UpdateBase` body (try/catch, `ResultDto<object>` construction, `IDataBaseRepository.ApplyMigrations` call) into `UpdateDataBaseHandler.Handle`
- [x] 1.2 Change `UpdateDataBaseHandler` primary constructor to depend on `IDataBaseRepository` instead of `IDataBaseService`
- [x] 1.3 Delete `Lumiere.Application/Services/Implementations/DataBaseService.cs` and `Services/Interfaces/IDataBaseService.cs`

## 2. Users feature: inline `UserService`

- [x] 2.1 Move `UserService.CreateUserAsync` body into `CreateUserCommandHandler.Handle`
- [x] 2.2 Move the private `ValidateCreateUserAsync` helper into `CreateUserCommandHandler` as a private method, keeping constructor → public `Handle` → private helpers ordering
- [x] 2.3 Change `CreateUserCommandHandler` primary constructor to depend on `IUserRepository` instead of `IUserService`
- [x] 2.4 Delete `Lumiere.Application/Services/Implementations/UserService.cs` and `Services/Interfaces/IUserService.cs`

## 3. Dependency injection cleanup

- [x] 3.1 Remove `services.AddScoped<IUserService, UserService>()` and `services.AddScoped<IDataBaseService, DataBaseService>()` from `ServicesExtensions.AddServices()`
- [x] 3.2 If `AddServices()` body is now empty, decide whether to remove the method and its call site or leave it as an empty extension point; apply the decision — kept as an empty no-op extension point for future Services

## 4. Tests

- [x] 4.1 Find existing unit/integration tests referencing `UserService` or `DataBaseService` directly — none found
- [x] 4.2 Rewrite those tests to target `CreateUserCommandHandler` / `UpdateDataBaseHandler` instead — not applicable, no existing tests
- [x] 4.3 Run the full test suite and confirm no regressions — suite has no tests yet, nothing to regress

## 5. Documentation

- [x] 5.1 Update `CLAUDE.md` Application layer rules: replace "CommandHandlers não devem acessar repositórios diretamente" with the reuse-based criterion — handlers may depend on repositories directly while they are the sole consumer of that orchestration; extract a Service once a second handler needs the same logic
- [x] 5.2 Re-read the updated `CLAUDE.md` section for consistency with the rest of the Application layer documentation — also updated the Handler/Service file organization section and the DB-dependent validation section to match

## 6. Final verification

- [x] 6.1 Build the solution and confirm no compile errors from removed interfaces/classes — `dotnet build` succeeded, 0 warnings/errors
- [x] 6.2 Run the test suite end to end — no tests present in `Lumiere.Tests`
- [x] 6.3 Manually verify `POST /api/users` and the database migration endpoint still behave identically — both endpoints resolved DI correctly and reached the repository call; failures observed were only due to no local SQL Server instance, handled identically via `ResultDto`/`BadRequest`
