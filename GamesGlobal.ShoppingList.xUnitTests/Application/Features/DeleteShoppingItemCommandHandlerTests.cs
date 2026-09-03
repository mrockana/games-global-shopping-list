using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Features;
using GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features.DeleteShoppingItem;

public sealed class DeleteShoppingItemCommandHandlerTests
{
    private readonly IApplicationRepository _repository = Substitute.For<IApplicationRepository>();
    private readonly ILogger<DeleteShoppingItemCommandHandler> _logger = Substitute.For<ILogger<DeleteShoppingItemCommandHandler>>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();

    [Fact]
    public async Task Handle_ItemDoesNotExist_ReturnsNotFoundException()
    {
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns((ShoppingItem?)null);

        // Act
        var result = await CreateHandler().Handle(new DeleteShoppingItemCommand(12, Guid.NewGuid()));

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainNotFoundException>(result.Error);
    }

    [Fact]
    public async Task Handle_ItemBelongsToAnotherUser_ReturnsForbiddenException()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Milk", Description = "Two litres" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);

        var result = await CreateHandler().Handle(new DeleteShoppingItemCommand(item.ShoppingItemId, Guid.NewGuid()));

        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
        _repository.DidNotReceive().Delete(item);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Milk", Description = "Two litres" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        // Act
        var result = await CreateHandler().Handle(new DeleteShoppingItemCommand(item.ShoppingItemId, item.UserCode));

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to delete shopping item.", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_ValidRequest_DeletesShoppingItem()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Milk", Description = "Two litres" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        // Act
        var result = await CreateHandler().Handle(new DeleteShoppingItemCommand(item.ShoppingItemId, item.UserCode));

        // Assert
        Assert.False(result.HasError);
        Assert.Equal(item.ShoppingItemId, result.Value?.ShoppingItemId);
        _repository.Received(1).Delete(item);
        await _cacheService.Received(1).RemoveAsync(ShoppingItemCacheKeys.ForUser(item.UserCode), Arg.Any<CancellationToken>());
    }

    [Theory]
    [ClassData(typeof(EmptyOrNegativePrimaryKeyId))]
    public void Validation_ShoppingItemIdIsEmptyOrNegative_ReturnsInvalid(long shoppingItemId)
    {
        var result = new DeleteShoppingItemValidation().Validate(new DeleteShoppingItemCommand(shoppingItemId, Guid.NewGuid()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Equals(nameof(DeleteShoppingItemCommand.ShoppingItemId), StringComparison.InvariantCulture));
    }

    private DeleteShoppingItemCommandHandler CreateHandler() => new(_repository, _logger, _cacheService);
}