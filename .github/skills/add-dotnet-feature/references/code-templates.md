# Code Templates — Add Feature Skill

Copy-paste starting points for each file. Replace `{FeatureName}`, `{EntityName}`, `{endpointUrl}`, and `{HttpMethod}` with the actual values.

---

## BusinessDomain — Entity

```csharp
// GamesGlobal.ShoppingList.BusinessDomain/Entities/{EntityName}.cs
using System.ComponentModel.DataAnnotations;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

namespace GamesGlobal.ShoppingList.BusinessDomain.Entities;

public sealed class {EntityName} : BaseEntity
{
    [Key]
    public long {EntityName}Id { get; set; }
    public long UserId { get; set; }

    [Required]
    public string? Name { get; set; }

    // Add additional properties derived from the response payload sample
}
```

---

## Infrastructure — EF Configuration

```csharp
// GamesGlobal.ShoppingList.Infrastructure/DataAccess/Application/EntityConfiguration/{EntityName}Configuration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.EntityConfiguration;

internal sealed class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.HasIndex(x => x.UserId)
            .IsClustered(false)
            .HasDatabaseName("IX_{EntityName}_UserId_NonClustered");
    }
}
```

### ApplicationDbContext addition

```csharp
// In ApplicationDbContext.cs — add inside the class:
public DbSet<{EntityName}> {EntityName}s { get; set; }

// In OnModelCreating — add:
modelBuilder.ApplyConfiguration(new {EntityName}Configuration());
```

---

## Application — Command Request

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandRequest.cs
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

public sealed record {FeatureName}CommandRequest(
    string EmailAddress,
    string Name          // Replace/extend with fields from request payload sample
) : ICommand<{FeatureName}Response>
{ }

internal static class {FeatureName}CommandRequestExtensions
{
    public static {EntityName} ToEntity(this {FeatureName}CommandRequest request)
    {
        return new {EntityName}
        {
            Name = request.Name
            // Map other fields
        };
    }
}
```

---

## Application — Query Request (GET)

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Query.cs
using GamesGlobal.ShoppingList.Application.Common;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

public sealed record {FeatureName}Query(
    string EmailAddress,
    long {EntityName}Id  // Replace with query parameters from endpoint URL
) : IQuery<{FeatureName}Response>
{ }
```

---

## Application — Response

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Response.cs
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

public sealed record class {FeatureName}Response(
    long {EntityName}Id,
    string Name          // Replace/extend with fields from response payload sample
)
{ }

public static class {FeatureName}ResponseExtensions
{
    public static {FeatureName}Response To{FeatureName}Response(this {EntityName} entity)
    {
        return new {FeatureName}Response(
            {EntityName}Id: entity.{EntityName}Id,
            Name: entity.Name!
            // Map other fields
        );
    }
}
```

---

## Application — Validator

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}Validation.cs
using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

public sealed class {FeatureName}Validation : AbstractValidator<{FeatureName}CommandRequest>
{
    public {FeatureName}Validation()
    {
        RuleFor(r => r.EmailAddress)
            .NotNull().NotEmpty()
            .WithMessage("EmailAddress is required");

        RuleFor(r => r.Name)
            .NotNull().NotEmpty()
            .WithMessage("Name is required");

        // Add rules for other required fields from the request payload sample
    }
}
```

---

## Application — Command Handler

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}CommandHandler.cs
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

internal sealed class {FeatureName}CommandHandler : IApplicationRequestHandler<{FeatureName}CommandRequest, {FeatureName}Response>
{
    private readonly IApplicationRepository _repository;

    public {FeatureName}CommandHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<{FeatureName}Response>> Handle(
        {FeatureName}CommandRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = request.ToEntity();

        var savedEntity = _repository.Insert(entity);
        await _repository.SaveAsync(cancellationToken);

        return new Result<{FeatureName}Response>(savedEntity.To{FeatureName}Response());
    }
}
```

---

## Application — Query Handler (GET)

```csharp
// GamesGlobal.ShoppingList.Application/Features/{FeatureName}/{FeatureName}QueryHandler.cs
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.Exceptions;

namespace GamesGlobal.ShoppingList.Application.Features.{FeatureName};

internal sealed class {FeatureName}QueryHandler : IApplicationRequestHandler<{FeatureName}Query, {FeatureName}Response>
{
    private readonly IApplicationRepository _repository;

    public {FeatureName}QueryHandler(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<{FeatureName}Response>> Handle(
        {FeatureName}Query request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetSingleAsync(
            new Find{EntityName}ById(request.{EntityName}Id), cancellationToken);

        if (entity is null)
            return new Result<{FeatureName}Response>(new DomainNotFoundException("{EntityName} not found"));

        return new Result<{FeatureName}Response>(entity.To{FeatureName}Response());
    }
}
```

---

## WebApi — Endpoint Request (POST/PUT body)

```csharp
// GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}EndpointRequest.cs
namespace GamesGlobal.ShoppingList.WebApi.Features.{FeatureName};

internal sealed record {FeatureName}EndpointRequest(
    string Name          // Replace/extend with fields from request payload sample
);
```

---

## WebApi — Endpoint (POST command example)

```csharp
// GamesGlobal.ShoppingList.WebApi/Features/{FeatureName}/{FeatureName}Endpoint.cs
using Microsoft.AspNetCore.Mvc;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.{FeatureName};
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Common.Tags;

namespace GamesGlobal.ShoppingList.WebApi.Features.{FeatureName};

internal sealed class {FeatureName}Endpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("{endpointUrl}",
            async ([FromBody] {FeatureName}EndpointRequest request,
                   [FromServices] ApplicationRequestProcessor requestProcessor,
                   HttpContext context) =>
            {
                var commandRequest = new {FeatureName}CommandRequest(
                    "user@example.com",   // Replace with real identity claim when IncludeIdentity is active
                    request.Name          // Map from endpoint request
                );

                var result = await requestProcessor.Process<{FeatureName}CommandRequest, {FeatureName}Response>(
                    commandRequest, context.RequestAborted);

                return result;
            })
            .WithName("{FeatureName}")
            .Produces<{FeatureName}Response>()
            .AddEndpointFilter<ResponseHandlingFilter>()
            .WithTags(EndpointTags.ShoppingItem); // Replace tag as appropriate
    }
}
```

---

## xUnit Test — Handler Tests

```csharp
// GamesGlobal.ShoppingList.xUnitTests/Application/{FeatureName}/{FeatureName}HandlerTests.cs
using NSubstitute;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Features.{FeatureName};
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using Xunit;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.{FeatureName};

public sealed class {FeatureName}HandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var repository = Substitute.For<IApplicationRepository>();
        var entity = new {EntityName} { {EntityName}Id = 1, Name = "Test" };

        repository.Insert(Arg.Any<{EntityName}>()).Returns(entity);
        repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new {FeatureName}CommandHandler(repository);
        var request = new {FeatureName}CommandRequest("user@example.com", "Test");

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.False(result.HasError);
        Assert.NotNull(result.Value);
        Assert.Equal("Test", result.Value!.Name);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsErrorResult()
    {
        // Arrange
        var repository = Substitute.For<IApplicationRepository>();
        var entity = new {EntityName} { {EntityName}Id = 1, Name = "Test" };
        repository.Insert(Arg.Any<{EntityName}>()).Returns(entity);
        repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        repository.SavedSuccessful(0).Returns(false);

        var handler = new {FeatureName}CommandHandler(repository);
        var request = new {FeatureName}CommandRequest("user@example.com", "Test");

        // Act
        var result = await handler.Handle(request);

        // Assert
        Assert.True(result.HasError);
    }
}
```
