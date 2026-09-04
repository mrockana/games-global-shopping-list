using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Features;
using GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Features.GetShoppingItems;

public sealed class GetShoppingItemsQueryHandlerTests
{
    private readonly IApplicationRepository _repository = Substitute.For<IApplicationRepository>();
    private readonly IIdentityRepository _identityRepository = Substitute.For<IIdentityRepository>();
    private readonly ILogger<GetShoppingItemsQueryHandler> _logger = Substitute.For<ILogger<GetShoppingItemsQueryHandler>>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IOptions<CacheOptions> _cacheOptions = Options.Create(new CacheOptions());

    [Fact]
    public async Task Handle_UserDoesNotExist_ReturnsApplicationException()
    {
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await CreateHandler().Handle(new GetShoppingItemsQuery(Guid.NewGuid()));

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
        Assert.Equal("Action Failed", result.Error?.Message);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsShoppingItems()
    {
        Guid userCode = Guid.NewGuid();
        var items = new List<ShoppingItem>
        {
            new() { ShoppingItemId = 12, UserCode = userCode, Name = "Milk", Description = "Two litres" },
            new() { ShoppingItemId = 34, UserCode = userCode, Name = "Bread", Description = "Wholemeal" },
        };
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns(new User { UserCode = userCode });
        _repository.GetAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>()).Returns(items);
        _cacheService.GetAsync<IList<GetShoppingItemResponse>>(ShoppingItemCacheKeys.ForUser(userCode), Arg.Any<CancellationToken>()).Returns((IList<GetShoppingItemResponse>?)null);

        var result = await CreateHandler().Handle(new GetShoppingItemsQuery(userCode));

        Assert.False(result.HasError);
        Assert.Equal(2, result.Value?.Count);
        Assert.Contains(result.Value!, item => item.ShoppingItemId == items[0].ShoppingItemId && string.Equals(item.Name, items[0].Name, StringComparison.Ordinal));
        await _cacheService.Received(1).SetAsync(ShoppingItemCacheKeys.ForUser(userCode), Arg.Any<IList<GetShoppingItemResponse>>(), TimeSpan.FromMinutes(15), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CacheHit_ReturnsCachedShoppingItemsWithoutQueryingRepository()
    {
        Guid userCode = Guid.NewGuid();
        IList<GetShoppingItemResponse> cachedItems = new List<GetShoppingItemResponse>
        {
            new(12, userCode, "Milk", "Two litres", []),
        };
        _identityRepository.GetSingleAsync(Arg.Any<Specification<User>>(), Arg.Any<CancellationToken>()).Returns(new User { UserCode = userCode });
        _cacheService.GetAsync<IList<GetShoppingItemResponse>>(ShoppingItemCacheKeys.ForUser(userCode), Arg.Any<CancellationToken>()).Returns(cachedItems);

        var result = await CreateHandler().Handle(new GetShoppingItemsQuery(userCode));

        Assert.False(result.HasError);
        Assert.Same(cachedItems, result.Value);
        await _repository.DidNotReceive().GetAsync(Arg.Any<Specification<ShoppingItem>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validation_UserCodeIsEmpty_ReturnsInvalid()
    {
        var result = new GetShoppingItemsValidation().Validate(new GetShoppingItemsQuery(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.Equals(nameof(GetShoppingItemsQuery.UserCode), StringComparison.Ordinal));
    }

    private GetShoppingItemsQueryHandler CreateHandler() => new(_repository, _logger, _identityRepository, _cacheService, _cacheOptions);
}