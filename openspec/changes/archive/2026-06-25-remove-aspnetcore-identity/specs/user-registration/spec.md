## ADDED Requirements

### Requirement: User registration request fields
The system SHALL accept exactly the following fields when registering a new user: `FirstName`, `LastName`, `Email`, `ConfirmEmail`, `Password`, `ConfirmPassword`. No `Username` field SHALL exist.

#### Scenario: Valid registration payload
- **WHEN** a client submits `POST /api/users` with `FirstName`, `LastName`, `Email`, `ConfirmEmail` equal to `Email`, `Password`, and `ConfirmPassword` equal to `Password`
- **THEN** the system creates the user and returns a success result

#### Scenario: Email confirmation mismatch
- **WHEN** `ConfirmEmail` does not match `Email`
- **THEN** the system rejects the request with a validation error and does not create the user

#### Scenario: Password confirmation mismatch
- **WHEN** `ConfirmPassword` does not match `Password`
- **THEN** the system rejects the request with a validation error and does not create the user

### Requirement: Email uniqueness
The system SHALL enforce that `Email` is unique across all users, both at the application level (pre-insert check) and at the database level (unique constraint).

#### Scenario: Duplicate email rejected
- **WHEN** a registration request uses an `Email` that already belongs to an existing user
- **THEN** the system rejects the request with a validation error and does not create a duplicate row

### Requirement: User entity has no framework dependency
The `User` entity SHALL NOT inherit from or reference any ASP.NET Core Identity type. Authentication-related data (e.g. password hash) SHALL be exposed on the entity only as primitive types, computed by the Infra layer.

#### Scenario: Domain compiles without Identity package
- **WHEN** the `Lumiere.Domain` project is built
- **THEN** it has no dependency on `Microsoft.AspNetCore.Identity` or any other ASP.NET Core package

### Requirement: Shared entity lifecycle fields via BaseEntity
All Domain entities SHALL inherit `CreatedAt`, `UpdatedAt`, `Active`, and the `Activate()`/`Deactivate()` behavior from an abstract `BaseEntity`, unless a documented exception applies.

#### Scenario: User and Channel inherit BaseEntity
- **WHEN** `User` or `Channel` is instantiated
- **THEN** `CreatedAt`, `UpdatedAt`, `Active` are available via the inherited `BaseEntity` members, not redeclared on the entity itself
