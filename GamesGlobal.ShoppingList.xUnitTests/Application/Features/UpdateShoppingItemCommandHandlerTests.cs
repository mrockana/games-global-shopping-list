using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.Application.Features;
using GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.xUnitTests.TestData;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features.UpdateShoppingItemCommand;

public sealed class UpdateShoppingItemCommandHandlerTests
{
    private readonly IApplicationRepository _repository = Substitute.For<IApplicationRepository>();
    private readonly ILogger<UpdateShoppingItemCommandHandler> _logger = Substitute.For<ILogger<UpdateShoppingItemCommandHandler>>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IEmbeddingService _embeddingService = Substitute.For<IEmbeddingService>();

    [Fact]
    public async Task Handle_ItemDoesNotExist_ReturnsNotFoundException()
    {
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns((ShoppingItem?)null);

        var result = await CreateHandler().Handle(new UpdateShoppingItemCommandRequest(12, Guid.NewGuid(), "Milk", "Two litres"));

        Assert.True(result.HasError);
        Assert.IsType<DomainNotFoundException>(result.Error);
    }

    [Fact]
    public async Task Handle_ItemBelongsToAnotherUser_ReturnsForbiddenException()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Bread", Description = "Wholemeal" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);

        var result = await CreateHandler().Handle(new UpdateShoppingItemCommandRequest(item.ShoppingItemId, Guid.NewGuid(), "Milk", "Two litres"));

        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
        Assert.Equal("Bread", item.Name);
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsApplicationException()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Bread", Description = "Wholemeal" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.SavedSuccessful(0).Returns(false);

        var result = await CreateHandler().Handle(new UpdateShoppingItemCommandRequest(item.ShoppingItemId, item.UserCode, "Milk", "Two litres"));

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Failed to update shopping item.", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesShoppingItem()
    {
        var item = new ShoppingItem { ShoppingItemId = 12, UserCode = Guid.NewGuid(), Name = "Bread", Description = "Wholemeal" };
        _repository.GetSingleAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(item);
        _repository.SaveAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.SavedSuccessful(1).Returns(true);

        var result = await CreateHandler().Handle(new UpdateShoppingItemCommandRequest(item.ShoppingItemId, item.UserCode, "Milk", "Two litres"));

        Assert.False(result.HasError);
        Assert.Equal("Milk", result.Value?.Name);
        Assert.Equal("Two litres", result.Value?.Description);
        Assert.Equal("Milk", item.Name);
        await _cacheService.Received(1).RemoveAsync(ShoppingItemCacheKeys.ForUser(item.UserCode), Arg.Any<CancellationToken>());
    }

    [Theory]
    [ClassData(typeof(EmptyOrNegativePrimaryKeyId))]
    public void Validation_ShoppingItemIdIsEmptyOrNegative_ReturnsInvalid(long shoppingItemId)
    {
        var result = new UpdateShoppingItemValidation().Validate(new UpdateShoppingItemCommandRequest(shoppingItemId, Guid.NewGuid(), "Milk", "Two litres"));

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("", "Two litres")]
    [InlineData("Milk", "")]
    public void Validation_NameOrDescriptionIsEmpty_ReturnsInvalid(string name, string description)
    {
        var result = new UpdateShoppingItemValidation().Validate(new UpdateShoppingItemCommandRequest(12, Guid.NewGuid(), name, description));

        Assert.False(result.IsValid);
    }

    private UpdateShoppingItemCommandHandler CreateHandler() => new(_repository, _logger, _cacheService, _embeddingService);
}