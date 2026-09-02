using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Logging;
namespace GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;

public sealed class CreateShoppingItemCommandHandler : IApplicationRequestHandler<CreateShoppingItemCommandRequest, CreateShoppingItemResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger<CreateShoppingItemCommandHandler> _logger;
    private readonly ActivitySource _activitySource;

    public CreateShoppingItemCommandHandler(IApplicationRepository repository, ILogger<CreateShoppingItemCommandHandler> logger, IIdentityRepository identityRepository)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _identityRepository = identityRepository;
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

        var itemToInsert = request.ToEntity();
        itemToInsert.UserCode = user.UserCode;

        ShoppingItem? domainModelResult = _repository.Insert(itemToInsert);
        int saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            _logger.LogError("Failed to create shopping item.");
            return Result.CreateErrorResult<CreateShoppingItemResponse>(new DomainApplicationException("Failed to create shopping item."));
        }

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
    public static ShoppingItem ToEntity(this CreateShoppingItemCommandRequest request)
    {
        return new ShoppingItem
        {
            Name = request.Name,
            Description = request.Description,
        };
    }
}