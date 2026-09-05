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
            .Where(shoppingItem => shoppingItem.UserCode == request.UserCode)
            .Select(shoppingItem => new
            {
                ShoppingItem = shoppingItem,
                IsFullTextMatch = EF.Functions
                    .ToTsVector("english", shoppingItem.Name + " " + shoppingItem.Description)
                    .Matches(EF.Functions.WebSearchToTsQuery("english", request.Search)),
                Distance = shoppingItem.Embeddings == null
                    ? 0D
                    : shoppingItem.Embeddings.L2Distance(embeddings[0]),
            })
            .Where(result => result.IsFullTextMatch || (result.ShoppingItem.Embeddings != null && 1D / (1D + result.Distance) >= MinimumConfidence))
            .OrderByDescending(result => result.IsFullTextMatch)
            .ThenBy(result => result.Distance)
            .Select(result => new SearchShoppingItemsResponse(
                result.ShoppingItem.ShoppingItemId,
                result.ShoppingItem.UserCode,
                result.ShoppingItem.Name!,
                result.ShoppingItem.Description,
                result.Distance,
                result.IsFullTextMatch ? 1D : 1D / (1D + result.Distance),
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