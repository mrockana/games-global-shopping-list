using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;

public sealed class CreateShoppingItemCommandHandler : IApplicationRequestHandler<CreateShoppingItemCommandRequest, CreateShoppingItemResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger<CreateShoppingItemCommandHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly ICacheService _cacheService;

    private readonly IEmbeddingService _embeddingService;

    public CreateShoppingItemCommandHandler(
        IApplicationRepository repository,
        ILogger<CreateShoppingItemCommandHandler> logger,
        IIdentityRepository identityRepository,
        ICacheService cacheService,
        IEmbeddingService embeddingService)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _identityRepository = identityRepository;
        _cacheService = cacheService;
        _embeddingService = embeddingService;
    }

    public async Task<Result<CreateShoppingItemResponse>> Handle(CreateShoppingItemCommandRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running : {nameof(CreateShoppingItemCommandHandler)}");
        var findUserbyUserCodeSpec = new FindUserByUserCode(request.UserCode).NoTracking();
        User? user = await _identityRepository.GetSingleAsync(findUserbyUserCodeSpec, cancellationToken);

        if (user is null)
        {
            return Result.CreateErrorResult<CreateShoppingItemResponse>(new DomainApplicationException("Failed to create shopping item."));
        }

        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object>>
        {
            new (nameof(request.UserCode), request.UserCode),
        });

        var textToEmbed = new List<string> { $"{request.Name} {request.Description}" };
        IReadOnlyList<Pgvector.Vector> embeddings = await _embeddingService.GenerateAsync(
            textToEmbed,
            cancellationToken);

        var itemToInsert = request.ToEntity(embeddings[0]);
        itemToInsert.UserCode = user.UserCode;

        ShoppingItem? domainModelResult = _repository.Insert(itemToInsert);
        int saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            _logger.LogError("Failed to create shopping item.");
            return Result.CreateErrorResult<CreateShoppingItemResponse>(new DomainApplicationException("Failed to create shopping item."));
        }

        await _cacheService.RemoveAsync(ShoppingItemCacheKeys.ForUser(user.UserCode), cancellationToken);
        Result<CreateShoppingItemResponse> response = Result.CreateResult<CreateShoppingItemResponse>(domainModelResult.ToCreateShoppingItemResponse());

        return response;
    }
}

public sealed record CreateShoppingItemCommandRequest(Guid UserCode, string Name, string? Description)
    : ICommand<CreateShoppingItemResponse>
{
}

public static class CreateShoppingItemCommandRequestExtensions
{
    public static ShoppingItem ToEntity(this CreateShoppingItemCommandRequest request, Pgvector.Vector embeddings)
    {
        return new ShoppingItem
        {
            Name = request.Name,
            Description = request.Description,
        };
    }
}