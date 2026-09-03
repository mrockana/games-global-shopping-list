using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;
using GamesGlobal.ShoppingList.BusinessDomain.Features.ShoppingItems;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;

public sealed class UploadShoppingItemImageCommandHandler : IApplicationRequestHandler<UploadShoppingItemImageCommandRequest, UploadShoppingItemImageResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly IIdentityRepository _identityRepository;
    private readonly IFileObjectStoreService _fileObjectStoreService;
    private readonly FileObjectStoreOptions _fileObjectStoreOptions;
    private readonly ILogger<UploadShoppingItemImageCommandHandler> _logger;
    private readonly ActivitySource _activitySource;

    public UploadShoppingItemImageCommandHandler(
        IApplicationRepository repository,
        IIdentityRepository identityRepository,
        IFileObjectStoreService fileObjectStoreService,
        FileObjectStoreOptions fileObjectStoreOptions,
        ILogger<UploadShoppingItemImageCommandHandler> logger)
    {
        _repository = repository;
        _identityRepository = identityRepository;
        _fileObjectStoreService = fileObjectStoreService;
        _fileObjectStoreOptions = fileObjectStoreOptions;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<UploadShoppingItemImageResponse>> Handle(UploadShoppingItemImageCommandRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running : {nameof(UploadShoppingItemImageCommandHandler)}");
        using var scope = _logger.BeginScope(new List<KeyValuePair<string, object>>
        {
            new (nameof(request.UserCode), request.UserCode),
            new (nameof(request.ShoppingItemId), request.ShoppingItemId),
        });

        var findUserByUserCodeSpec = new FindUserByUserCode(request.UserCode).NoTracking();
        User? user = await _identityRepository.GetSingleAsync(findUserByUserCodeSpec, cancellationToken);

        var failedToUploadMessage = "Failed to upload shopping item image.";
        if (user is null)
        {
            _logger.LogError(failedToUploadMessage);
            return Result.CreateErrorResult<UploadShoppingItemImageResponse>(new DomainApplicationException(failedToUploadMessage));
        }

        var findShoppingItemByIdSpec = new FindShoppingItemById(request.ShoppingItemId);
        findShoppingItemByIdSpec.Include(shoppingItem => shoppingItem.Documents);

        ShoppingItem? item = await _repository.GetSingleAsync(findShoppingItemByIdSpec, cancellationToken);

        if (item is null)
        {
            _logger.LogError(failedToUploadMessage);
            return Result.CreateErrorResult<UploadShoppingItemImageResponse>(new DomainNotFoundException("Shopping item was not found."));
        }

        if (item.UserCode != user.UserCode)
        {
            _logger.LogWarning("A user attempted to upload an image to a shopping item they do not own.");
            return Result.CreateErrorResult<UploadShoppingItemImageResponse>(new DomainForbiddenActionException("You are not allowed to upload an image for this shopping item."));
        }

        string bucketName = _fileObjectStoreOptions.BucketName;
        string objectName = BuildObjectName(user.UserCode, item.ShoppingItemId, request.FileName);

        if (!await _fileObjectStoreService.BucketExists(bucketName, cancellationToken))
        {
            await _fileObjectStoreService.CreateBucket(bucketName);
        }

        await _fileObjectStoreService.UploadObject(bucketName, objectName, request.Content, request.ContentType, request.Length, cancellationToken);

        var document = new Document
        {
            Name = Path.GetFileName(request.FileName),
            MimeType = request.ContentType,
            Size = (int)request.Length,
            Url = BuildUrl(_fileObjectStoreOptions.Url, bucketName, objectName),
        };

        item.Documents.Add(document);

        int saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            _logger.LogError(failedToUploadMessage);
            await _fileObjectStoreService.RemoveObject(bucketName, objectName, cancellationToken);
            return Result.CreateErrorResult<UploadShoppingItemImageResponse>(new DomainApplicationException(failedToUploadMessage));
        }

        return Result.CreateResult(document.ToUploadShoppingItemImageResponse(item.ShoppingItemId));
    }

    private static string BuildObjectName(Guid userCode, long shoppingItemId, string fileName)
    {
        string leafName = Path.GetFileName(fileName);
        return string.Create(CultureInfo.InvariantCulture, $"{userCode}/{shoppingItemId}/{leafName}");
    }

    private static string BuildUrl(string storeUrl, string bucketName, string objectName)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{storeUrl.TrimEnd('/')}/{bucketName}/{objectName}");
    }
}

public sealed record UploadShoppingItemImageCommandRequest(
    Guid UserCode,
    long ShoppingItemId,
    string FileName,
    Stream Content,
    string ContentType,
    long Length)
    : ICommand<UploadShoppingItemImageResponse>
{
}
