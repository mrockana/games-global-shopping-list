using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;

public sealed class UpdateShoppingItemCommandHandler : IApplicationRequestHandler<UpdateShoppingItemCommandRequest, UpdateShoppingItemResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ILogger<UpdateShoppingItemCommandHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly ICacheService _cacheService;

    private readonly IEmbeddingService _embeddingService;

    public UpdateShoppingItemCommandHandler(
        IApplicationRepository repository,
        ILogger<UpdateShoppingItemCommandHandler> logger,
        ICacheService cacheService,
        IEmbeddingService embeddingService)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _cacheService = cacheService;
        _embeddingService = embeddingService;
    }

    public async Task<Result<UpdateShoppingItemResponse>> Handle(UpdateShoppingItemCommandRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(UpdateShoppingItemCommandHandler)}");
        var updateCounter = DiagnosticConfig.UpdateShoppingItemCounter;

        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object>>
        {
            new(nameof(request.ShoppingItemId), request.ShoppingItemId),
        });

        updateCounter.Add(1, new KeyValuePair<string, object?>(nameof(request.ShoppingItemId), request.ShoppingItemId));

        FindShoppingItemById findShoppingItemByIdSpecification = new FindShoppingItemById(request.ShoppingItemId);
        ShoppingItem? shoppingItem = await _repository.GetSingleAsync(findShoppingItemByIdSpecification, cancellationToken);

        if (shoppingItem == null)
        {
            return Result.CreateErrorResult<UpdateShoppingItemResponse>(new DomainNotFoundException($"Shopping item with ID {request.ShoppingItemId.ToString()} not found."));
        }

        if (shoppingItem.UserCode != request.UserCode)
        {
            return Result.CreateErrorResult<UpdateShoppingItemResponse>(new DomainForbiddenActionException("You are not allowed to update this shopping item."));
        }

        var textToEmbed = new List<string> { $"{request.Name} {request.Description}" };
        IReadOnlyList<Pgvector.Vector> embeddings = await _embeddingService.GenerateAsync(
            textToEmbed,
            cancellationToken);

        shoppingItem!.Description = request.Description;
        shoppingItem.Name = request.Name;
        shoppingItem.Embeddings = embeddings[0];

        int saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            _logger.LogError("Failed to update shopping item with ID {ShoppingItemId}.", request.ShoppingItemId);
            return Result.CreateErrorResult<UpdateShoppingItemResponse>(new DomainApplicationException("Failed to update shopping item."));
        }

        await _cacheService.RemoveAsync(ShoppingItemCacheKeys.ForUser(shoppingItem.UserCode), cancellationToken);
        Result<UpdateShoppingItemResponse>? response = Result.CreateResult<UpdateShoppingItemResponse>(shoppingItem.ToUpdateShoppingItemResponse());
        return response;
    }
}

public sealed record UpdateShoppingItemCommandRequest(long ShoppingItemId, Guid UserCode, string Name, string? Description)
    : ICommand<UpdateShoppingItemResponse>
{
}