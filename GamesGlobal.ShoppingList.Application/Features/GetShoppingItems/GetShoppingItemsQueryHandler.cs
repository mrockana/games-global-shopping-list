using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;

public sealed class GetShoppingItemsQueryHandler : IApplicationRequestHandler<GetShoppingItemsQuery, IList<GetShoppingItemResponse>>
{
    private readonly IApplicationRepository _appRepository;
    private readonly ILogger<GetShoppingItemsQueryHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IIdentityRepository _identityRepository;
    private readonly ICacheService _cacheService;
    private readonly CacheOptions _cacheOptions;

    public GetShoppingItemsQueryHandler(
        IApplicationRepository appRepository,
        ILogger<GetShoppingItemsQueryHandler> logger,
        IIdentityRepository identityRepository,
        ICacheService cacheService,
        IOptions<CacheOptions> cacheOptions)
    {
        _appRepository = appRepository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _identityRepository = identityRepository;
        _cacheService = cacheService;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<Result<IList<GetShoppingItemResponse>>> Handle(GetShoppingItemsQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(GetShoppingItemsQueryHandler)}", ActivityKind.Server);
        _logger.LogInformation("Get Shopping Items");

        var findUserbyIdSpec = new FindUserByUserCode(request.UserCode).NoTracking();
        User? user = await _identityRepository.GetSingleAsync(findUserbyIdSpec, cancellationToken);
        if (user is null)
        {
            return Result.CreateErrorResult<IList<GetShoppingItemResponse>>(new DomainApplicationException("Action Failed"));
        }

        string cacheKey = ShoppingItemCacheKeys.ForUser(user.UserCode);
        IList<GetShoppingItemResponse>? cachedShoppingItems = await _cacheService.GetAsync<IList<GetShoppingItemResponse>>(cacheKey, cancellationToken);

        if (cachedShoppingItems is not null)
        {
            return Result.CreateResult(cachedShoppingItems);
        }

        var findShoppingItemsByUserIdSpecification = new FindShoppingItemByUserCode(user!.UserCode)
            .Include(s => s.Documents)
            .NoTracking();
        var shoppingItemsDomain = await _appRepository.GetAsync(findShoppingItemsByUserIdSpecification, cancellationToken);

        var shoppingItems = shoppingItemsDomain.Select(d => d.ToGetShoppingItemResponse()).ToList();
        await _cacheService.SetAsync(cacheKey, shoppingItems, TimeSpan.FromMinutes(_cacheOptions.ShoppingItemsTtlMinutes), cancellationToken);
        var result = Result.CreateResult<IList<GetShoppingItemResponse>>(shoppingItems);

        return result;
    }
}

public sealed record GetShoppingItemsQuery(Guid UserCode)
    : IQuery<IList<GetShoppingItemResponse>>
{
}