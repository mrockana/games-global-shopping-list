using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector.EntityFrameworkCore;

namespace GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;

public sealed class SearchShoppingItemsQueryHandler : IApplicationRequestHandler<SearchShoppingItemsQuery, IList<SearchShoppingItemsResponse>>
{
    private const double MinimumConfidence = 0.45D;

    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ILogger<SearchShoppingItemsQueryHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IEmbeddingService _embeddingService;

    public SearchShoppingItemsQueryHandler(
        IApplicationDbContext applicationDbContext,
        ILogger<SearchShoppingItemsQueryHandler> logger,
        IEmbeddingService embeddingService)
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _embeddingService = embeddingService;
    }

    public async Task<Result<IList<SearchShoppingItemsResponse>>> Handle(SearchShoppingItemsQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(SearchShoppingItemsQueryHandler)}", ActivityKind.Server);
        _logger.LogInformation("Get Shopping Items");

        IReadOnlyList<Pgvector.Vector> embeddings = await _embeddingService.GenerateAsync([request.Search], cancellationToken);

        var shoppingItems = await _applicationDbContext.ShoppingItems
            .AsNoTracking()
            .Where(shoppingItem => shoppingItem.UserCode == request.UserCode && shoppingItem.Embeddings != null)
            .Select(shoppingItem => new
            {
                ShoppingItem = shoppingItem,
                Distance = shoppingItem.Embeddings!.L2Distance(embeddings[0]),
            })
            .Where(result => 1D / (1D + result.Distance) >= MinimumConfidence)
            .OrderBy(result => result.Distance)
            .Select(result => new SearchShoppingItemsResponse(
                result.ShoppingItem.ShoppingItemId,
                result.ShoppingItem.UserCode,
                result.ShoppingItem.Name!,
                result.ShoppingItem.Description,
                result.Distance,
                1D / (1D + result.Distance),
                result.ShoppingItem.Documents
                    .Select(document => new SearchShoppingItemsDocumentResponse(document.DocumentId, document.MimeType, document.Url, document.Name, document.Size))
                    .ToList()))
            .ToListAsync(cancellationToken);
        return Result.CreateResult<IList<SearchShoppingItemsResponse>>(shoppingItems);
    }
}

public sealed record SearchShoppingItemsQuery(Guid UserCode, string Search)
    : IQuery<IList<SearchShoppingItemsResponse>>
{
}