using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.Application.Features;
using GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features.CreateShoppingItem;

public sealed class CreateShoppingItemCommandHandlerTests
{
    private readonly IApplicationRepository _repository = Substitute.For<IApplicationRepository>();
    private readonly IIdentityRepository _identityRepository = Substitute.For<IIdentityRepository>();
    private readonly ILogger<CreateShoppingItemCommandHandler> _logger = Substitute.For<ILogger<CreateShoppingItemCommandHandler>>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IEmbeddingService _embeddingService = Substitute.For<IEmbeddingService>();

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsApplicationException()
    {
        var command = new CreateShoppingItemCommandRequest(Guid.NewGuid(), "Milk", "Two litres");
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await CreateHandler().Handle(command);

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to create shopping item.", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        Guid userCode = Guid.NewGuid();
        var command = new CreateShoppingItemCommandRequest(userCode, "Milk", "Two litres");
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns(new User { UserCode = userCode });
        _repository.Insert(Arg.Any<ShoppingItem>()).Returns(callInfo => callInfo.Arg<ShoppingItem>());
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        // Act
        var result = await CreateHandler().Handle(command);

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesShoppingItem()
    {
        Guid userCode = Guid.NewGuid();
        var command = new CreateShoppingItemCommandRequest(userCode, "Milk", "Two litres");
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = userCode, Name = command.Name, Description = command.Description };
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns(new User { UserCode = userCode });

        _repository.Insert(Arg.Any<ShoppingItem>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        // Act
        var result = await CreateHandler().Handle(command);

        // Assert
        Assert.False(result.HasError);
        Assert.Equal(item.ShoppingItemId, result.Value?.ShoppingItemId);
        Assert.Equal(userCode, result.Value?.UserCode);
        Assert.Equal(command.Name, result.Value?.Name);
        _repository.Received(1).Insert(Arg.Is<ShoppingItem>(shoppingItem => shoppingItem.UserCode == userCode && shoppingItem.Name == command.Name));
        await _cacheService.Received(1).RemoveAsync(ShoppingItemCacheKeys.ForUser(userCode), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("Name", "")]
    public void Validation_NameOrDescriptionIsEmpty_ReturnsInvalid(string name, string description)
    {
        var result = new CreateShoppingItemValidation().Validate(new CreateShoppingItemCommandRequest(Guid.NewGuid(), name, description));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validation_UserCodeIsEmpty_ReturnsInvalid()
    {
        var result = new CreateShoppingItemValidation().Validate(new CreateShoppingItemCommandRequest(Guid.Empty, "Milk", "Two litres"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Equals(nameof(CreateShoppingItemCommandRequest.UserCode), StringComparison.InvariantCulture));
    }

    private CreateShoppingItemCommandHandler CreateHandler() => new(_repository, _logger, _identityRepository, _cacheService, _embeddingService);
}