using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;

public sealed class DeleteShoppingItemCommandHandler : IApplicationRequestHandler<DeleteShoppingItemCommand, DeleteShoppingItemResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ILogger<DeleteShoppingItemCommandHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly ICacheService _cacheService;

    public DeleteShoppingItemCommandHandler(
        IApplicationRepository repository,
        ILogger<DeleteShoppingItemCommandHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _cacheService = cacheService;
    }

    public async Task<Result<DeleteShoppingItemResponse>> Handle(DeleteShoppingItemCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(DeleteShoppingItemCommandHandler)}");

        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object>>
        {
            new (nameof(request.ShoppingItemId), request.ShoppingItemId),
        });

        var findShoppingItemByIdSpecification = new FindShoppingItemById(request.ShoppingItemId).NoTracking();
        ShoppingItem? model = await _repository.GetSingleAsync(findShoppingItemByIdSpecification, cancellationToken);

        if (model is null)
        {
            return Result.CreateErrorResult<DeleteShoppingItemResponse>(new DomainNotFoundException($"Shopping item with ID {request.ShoppingItemId.ToString()} not found."));
        }

        if (model.UserCode != request.UserCode)
        {
            return Result.CreateErrorResult<DeleteShoppingItemResponse>(new DomainForbiddenActionException("You are not allowed to delete this shopping item."));
        }

        _repository.Delete(model!);
        int saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            _logger.LogError("Failed to delete shopping item with ID {ShoppingItemId}.", request.ShoppingItemId);
            return Result.CreateErrorResult<DeleteShoppingItemResponse>(new DomainApplicationException("Failed to delete shopping item."));
        }

        await _cacheService.RemoveAsync(ShoppingItemCacheKeys.ForUser(model.UserCode), cancellationToken);
        Result<DeleteShoppingItemResponse> response = Result.CreateResult<DeleteShoppingItemResponse>(model.ToDeleteShoppingItemResponse());

        return response;
    }
}

public sealed record DeleteShoppingItemCommand(long ShoppingItemId, Guid UserCode)
    : ICommand<DeleteShoppingItemResponse>
{
}