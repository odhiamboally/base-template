# Feature Folder Convention

This solution follows a modular monolith structure where feature-owned code lives under:

```text
Features/{BoundedContext}/{Feature}
```

Use this convention consistently across Application, Domain, Persistence, and Infrastructure when code has a clear business owner. This keeps a feature easy to find in every layer without mixing unrelated concerns.

## Ownership Rules

- Banking owns customer business capabilities, including customer entities, customer commands/queries, customer repositories, customer email consumers, and customer number generation.
- HR owns employee business capabilities, including employee entities, employee repositories, employee events, and employee number generation.
- IAM owns identity and access capabilities, including AppUser services, claims, JWT, sessions, identity resolution, user context, and authentication-oriented SMS composition.
- Shared owns reusable platform/business-support capabilities such as notifications, email templates, lookups, outbox, failed messages, integration event publishing, caching, encryption, and background jobs.

## Feature Contracts

When an interface or implementation belongs to one feature, place it inside that feature:

```text
Features/{BoundedContext}/{Feature}/Contracts/Interfaces
Features/{BoundedContext}/{Feature}/Contracts/Implementations
```

Examples:

- `Features/Banking/Customers/Contracts/Interfaces/ICustomerNumberGenerator.cs`
- `Features/Banking/Customers/Contracts/Implementations/CustomerNumberGenerator.cs`
- `Features/IAM/Users/Contracts/Interfaces/IJwtService.cs`
- `Features/IAM/Users/Contracts/Implementations/Services/JwtService.cs`

Avoid generic feature contracts when the business meaning is specific. For example, prefer `ICustomerNumberGenerator` and `IEmployeeNumberGenerator` over a shared `INumberGenerator`.

## Cross-Cutting Exceptions

Do not force everything into `Features`. Keep genuinely cross-cutting code outside feature folders, preferably in existing `Common`, `Shared`, or infrastructure-level folders.

Examples:

- Generic caching abstractions and behaviors
- Logging utilities and `LoggerMessage` definitions shared by many features
- Middleware
- JSON and serialization utilities
- Base entities
- Generic repository/specification abstractions
- Transactional unit-of-work abstractions
- Generic integration event publishing

## Dependency Injection

Register feature-owned services close to their owning module/context where possible. Shared or platform services can remain in shared infrastructure registration.

Prefer explicit feature-specific registrations:

```csharp
services.AddScoped<ICustomerNumberGenerator, CustomerNumberGenerator>();
services.AddScoped<IEmployeeNumberGenerator, EmployeeNumberGenerator>();
```

Use `Lazy<T>` only when it solves a real circular dependency or expensive construction problem. It should not be the default DI pattern.

## Logging And Error Handling

Every `catch` block should log or deliberately translate the exception. Prefer existing `LoggerMessage` definitions, and add new definitions when a feature needs new structured log events.

## Repository Pattern

Keep the repository/unit-of-work pattern for now. In this solution it is doing more than wrapping EF Core: it helps isolate multiple DbContexts, bounded-context units of work, specifications, domain-event dispatch, and outbox/persistence concerns.
