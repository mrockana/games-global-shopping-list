---
name: add-dotnet-feature
description: "Use when adding a new feature or endpoint to the TemplateAPI .NET clean architecture solution. Covers all layers: WebApi endpoint, Application command/query + handler + validator, BusinessDomain entity, Infrastructure EF configuration, and xUnit tests. Triggers: add feature, new endpoint, new command, new query, implement feature, create API endpoint."
argument-hint: "Feature name or brief description (e.g. 'create order')"
---

# Add a New Feature — GamesGlobal.ShoppingList

## When to Use
- Adding a new HTTP endpoint to the API
- Implementing a new command (write operation) or query (read operation)
- Scaffolding all clean-architecture layers for a new capability

---

## Step 1 — Gather Requirements (REQUIRED, ask first)

Before writing any code, ask the user for the following. Do NOT skip or assume any of these:

1. **Feature Name** — PascalCase verb-noun (e.g. `CreateOrder`, `GetOrderById`, `DeleteShoppingItem`)
2. **Endpoint URL** — the HTTP route (e.g. `/create-order`, `/orders/{id}`)
3. **HTTP Method** — GET, POST, PUT, or DELETE
4. **Request payload sample** — JSON example of what the caller sends (or "none" for GET/DELETE with no body)
5. **Response payload sample** — JSON example of the expected response body


Use the `vscode_askQuestions` tool to collect all five answers in a single prompt.

Derive from the answers:
- **Operation type**: GET → `IQuery<T>`; POST/PUT/DELETE → `ICommand<T>`
- **Entity name**: usually the noun in the Feature Name (e.g. `Order` from `CreateOrder`)
- **Is new entity?** Ask if the entity/DbSet does not already exist in `ApplicationDbContext`.

---

## Step 2 — Confirm Plan

Show the user a concise list of files that will be created/modified and ask for confirmation before proceeding.

```
Files to CREATE:
  WebApi:
    GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}Endpoint.cs
    GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}EndpointRequest.cs   (if body exists)
  Application:
    GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandRequest.cs   (or ...Query.cs)
    GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandHandler.cs  (or ...QueryHandler.cs)
    GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Validation.cs
    GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Response.cs
  BusinessDomain (if new entity):
    GamesGlobal.ShoppingList.BusinessDomain/Entities/{EntityName}.cs
  Infrastructure (if new entity):
    GamesGlobal.ShoppingList.Infrastructure/DataAccess/Application/EntityConfiguration/{EntityName}Configuration.cs
  Tests:
    GamesGlobal.ShoppingList.xUnitTests/Application/{FeatureName}/{FeatureName}HandlerTests.cs

Files to MODIFY (if new entity):
    GamesGlobal.ShoppingList.Infrastructure/DataAccess/Application/ApplicationDbContext.cs
      → add: public DbSet<{EntityName}> {EntityName}s { get; set; }
```

---

## Step 3 — Scaffold Files

Follow the layer-by-layer procedure below. Use the [code templates reference](./references/code-templates.md) for exact file patterns.

### 3a. BusinessDomain — Entity (skip if entity already exists)

File: `GamesGlobal.ShoppingList.BusinessDomain/Entities/{EntityName}.cs`

- Inherit from `BaseEntity`
- Add `[Key]` on the primary key (`long {EntityName}Id`)
- Add `[Required]` on non-nullable string properties
- Use `sealed class`
- Namespace: `GamesGlobal.ShoppingList.BusinessDomain.Entities`

### 3b. Infrastructure — EF Configuration (skip if entity already exists)

File: `GamesGlobal.ShoppingList.Infrastructure/DataAccess/Application/EntityConfiguration/{EntityName}Configuration.cs`

- Implement `IEntityTypeConfiguration<{EntityName}>`
- Configure indexes relevant to query patterns
- Namespace: `GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.EntityConfiguration`

Modify `ApplicationDbContext.cs`:
- Add `public DbSet<{EntityName}> {EntityName}s { get; set; }`
- Add `modelBuilder.ApplyConfiguration(new {EntityName}Configuration());` in `OnModelCreating`

### 3c. Application — Command/Query Request

File: `GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandRequest.cs`  
(or `{FeatureName}Query.cs` for GET)

- Use `sealed record`
- Implement `ICommand<{FeatureName}Response>` for writes, `IQuery<{FeatureName}Response>` for reads
- Map request fields from the user-provided payload sample
- Namespace: `GamesGlobal.ShoppingList.Application.Features.{FeatureName}`

### 3d. Application — Response

File: `GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Response.cs`

- Use `sealed record class`
- Add a static extension class `{FeatureName}ResponseExtensions` in the same file
- Add `To{FeatureName}Response(this {EntityName} entity)` extension method
- Map fields from the user-provided response payload sample
- Namespace: `GamesGlobal.ShoppingList.Application.Features.{FeatureName}`

### 3e. Application — Validator

File: `GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Validation.cs`

- Inherit `AbstractValidator<{FeatureName}CommandRequest>` (or Query)
- Add `RuleFor` for every non-nullable field with `.NotNull().NotEmpty().WithMessage("...")`
- Use `sealed class`
- Namespace: `GamesGlobal.ShoppingList.Application.Features.{FeatureName}`

### 3f. Application — Handler

File: `GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandHandler.cs`

- Use `sealed class`
- Implement `IApplicationRequestHandler<{FeatureName}CommandRequest, {FeatureName}Response>`
- Inject `IApplicationRepository` (and `IApplicationRepository` for identity if needed)
- For writes: `_repository.Insert(entity)` then `await _repository.SaveAsync()`
- For reads: `await _repository.GetSingleAsync(new Find{EntityName}By...())`
- Return `new Result<{FeatureName}Response>(entity.To{FeatureName}Response())`
- Namespace: `GamesGlobal.ShoppingList.Application.Features.{FeatureName}`

### 3g. WebApi — Endpoint Request (skip for GET/DELETE with no body)

File: `GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}EndpointRequest.cs` And `GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}.http`

- Use `sealed record`
- Fields derived from the user-provided request payload sample
- Namespace: `GamesGlobal.ShoppingList.WebApi.Features.{FeatureName}`
- The `.http` file is a copy of the request payload sample for easy reference when implementing the endpoint

### 3h. WebApi — Endpoint

File: `GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}Endpoint.cs`

- Use `sealed class`, implement `IEndpoint`
- Use `app.Map{HttpMethod}("{endpointUrl}", ...)` with the correct verb
- Inject `[FromBody] {FeatureName}EndpointRequest request` (POST/PUT) or route/query params (GET/DELETE)
- Inject `[FromServices] ApplicationRequestProcessor requestProcessor`
- Inject `HttpContext context`
- Create the command/query request and call `requestProcessor.Process<...>(request, context.RequestAborted)`
- Return the result directly (the `ResponseHandlingFilter` handles HTTP status codes)
- Chain `.WithName("{FeatureName}")`, `.Produces<{FeatureName}Response>()`, `.AddEndpointFilter<ResponseHandlingFilter>()`, `.WithTags(EndpointTags.{Category})`
- No DI registration needed — endpoints are auto-discovered via `AddEndpoints()`
- Namespace: `GamesGlobal.ShoppingList.WebApi.Features.{FeatureName}`

---

## Step 4 — Scaffold Unit Tests

File: `GamesGlobal.ShoppingList.xUnitTests/Application/{FeatureName}/{FeatureName}HandlerTests.cs`

- Use `sealed class` with xUnit `[Fact]` methods
- Use NSubstitute for mocking (`Substitute.For<IApplicationRepository>()`)
- Cover at minimum:
  - Happy path: handler returns success result
  - Not found path (if applicable): entity missing → returns error result
  - Validation failure: invalid request → `DomainValidationException` via `ApplicationRequestProcessor`
- Follow the pattern in `ApplicationRequestProcessorTestsHelper`
- Namespace: `GamesGlobal.ShoppingList.xUnitTests.Application.{FeatureName}`

---

## Step 5 — Verify

Run the build to confirm no compilation errors:

```
dotnet build .\GamesGlobal.ShoppingList.sln
```

Run tests:

```
dotnet test .\GamesGlobal.ShoppingList.sln
```

If build errors exist, fix them before presenting the result to the user.

---

## Key Conventions Reference

| Concept | Convention |
|---|---|
| Command request | `sealed record {Name}CommandRequest(...) : ICommand<{Name}Response>` |
| Query request | `sealed record {Name}Query(...) : IQuery<{Name}Response>` |
| Handler | `sealed class {Name}CommandHandler : IApplicationRequestHandler<{Name}CommandRequest, {Name}Response>` |
| Validator | `sealed class {Name}Validation : AbstractValidator<{Name}CommandRequest>` |
| Response | `sealed record class {Name}Response(...)` |
| Entity | `sealed class {EntityName} : BaseEntity` with `[Key] public long {EntityName}Id` |
| Endpoint | `sealed class {Name}Endpoint : IEndpoint` — auto-discovered, no DI registration needed |
| Namespaces | `GamesGlobal.ShoppingList.{Layer}.Features.{FeatureName}` |
| editorConfig | `C:\DevWork\dotnet-web-api-template\.editorconfig` — use this to follow best practices |

See [code templates reference](./references/code-templates.md) for full copy-paste templates.
