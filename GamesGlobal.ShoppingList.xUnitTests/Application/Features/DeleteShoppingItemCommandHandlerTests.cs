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

    [Fact]
    public async Task Handle_ItemDoesNotExist_ReturnsNotFoundException()
    {
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns((ShoppingItem?)null);

        // Act
        var result = await new DeleteShoppingItemCommandHandler(_repository, _logger).Handle(new DeleteShoppingItemCommand(12));

        // Assert
        Assert.True(result.HasError);
        Assert.IsType<DomainNotFoundException>(result.Error);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, Name = "Milk", Description = "Two litres" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        // Act
        var result = await new DeleteShoppingItemCommandHandler(_repository, _logger).Handle(new DeleteShoppingItemCommand(item.ShoppingItemId));

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
        var result = await new DeleteShoppingItemCommandHandler(_repository, _logger).Handle(new DeleteShoppingItemCommand(item.ShoppingItemId));

        // Assert
        Assert.False(result.HasError);
        Assert.Equal(item.ShoppingItemId, result.Value?.ShoppingItemId);
        _repository.Received(1).Delete(item);
    }

    [Theory]
    [ClassData(typeof(EmptyOrNegativePrimaryKeyId))]
    public void Validation_ShoppingItemIdIsEmptyOrNegative_ReturnsInvalid(long shoppingItemId)
    {
        var result = new DeleteShoppingItemValidation().Validate(new DeleteShoppingItemCommand(shoppingItemId));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Equals(nameof(DeleteShoppingItemCommand.ShoppingItemId), StringComparison.InvariantCulture));
    }
}