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
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ILogger<SearchShoppingItemsQueryHandler> _logger;
    private readonly ActivitySource _activitySource;
    private readonly IEmbeddingService _embeddingService;
    private readonly ShoppingItemsOptions _shoppingItemsOptions;

    public SearchShoppingItemsQueryHandler(
        IApplicationDbContext applicationDbContext,
        ILogger<SearchShoppingItemsQueryHandler> logger,
        IEmbeddingService embeddingService,
        ShoppingItemsOptions shoppingItemsOptions)
    {
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
        _embeddingService = embeddingService;
        _shoppingItemsOptions = shoppingItemsOptions;
    }

    public async Task<Result<IList<SearchShoppingItemsResponse>>> Handle(SearchShoppingItemsQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(SearchShoppingItemsQueryHandler)}", ActivityKind.Server);
        _logger.LogInformation("Get Shopping Items");
        bool isVectorSearchFeatureFlag = _shoppingItemsOptions.EnableVectorSearch;
        KeyValuePair<string, object?>[] tags =
        [
            new("UserCode", request.UserCode.ToString()),
            new("SearchType", isVectorSearchFeatureFlag ? "Vector" : "FullText"),
        ];

        DiagnosticConfig.SearchShoppingItemsCounter.Add(1, tags);

        var searchStopwatch = Stopwatch.StartNew();
        try
        {
            var fullTextSearchQueryable = _applicationDbContext.ShoppingItems
                .AsNoTracking()
                .Where(shoppingItem => shoppingItem.UserCode == request.UserCode)
                .Select(shoppingItem => new
                {
                    ShoppingItem = shoppingItem,
                    IsFullTextMatch = EF.Functions
                        .ToTsVector("english", shoppingItem.Name + " " + shoppingItem.Description)
                        .Matches(EF.Functions.WebSearchToTsQuery("english", request.SearchText)),
                });

            // Feature Flag
            if (isVectorSearchFeatureFlag)
            {
                _logger.LogInformation("Vector Enabled Search");
                IReadOnlyList<Pgvector.Vector> embeddings = await _embeddingService.GenerateAsync([request.SearchText], cancellationToken);

                var vectorSearchQueryable = fullTextSearchQueryable
                    .Select(result => new
                    {
                        result.ShoppingItem,
                        result.IsFullTextMatch,
                        Distance = result.ShoppingItem.Embeddings == null
                            ? 0D
                            : result.ShoppingItem.Embeddings.L2Distance(embeddings[0]),
                    })
                    .Where(result => result.IsFullTextMatch || (result.ShoppingItem.Embeddings != null && 1D / (1D + result.Distance) >= _shoppingItemsOptions.MinimumConfidence))
                    .OrderByDescending(result => result.IsFullTextMatch)
                    .ThenBy(result => result.Distance);

                var vectorSearchResults = await vectorSearchQueryable
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

                return Result.CreateResult<IList<SearchShoppingItemsResponse>>(vectorSearchResults);
            }

            var fullTextResultsQueryable = fullTextSearchQueryable
                .Where(result => result.IsFullTextMatch)
                .Select(result => new SearchShoppingItemsResponse(
                    result.ShoppingItem.ShoppingItemId,
                    result.ShoppingItem.UserCode,
                    result.ShoppingItem.Name!,
                    result.ShoppingItem.Description,
                    0D,
                    1D,
                    result.ShoppingItem.Documents
                        .Select(document => new SearchShoppingItemsDocumentResponse(document.DocumentId, document.MimeType, document.Url, document.Name, document.Size))
                        .ToList()));

            var results = await fullTextResultsQueryable.ToListAsync(cancellationToken);
            return Result.CreateResult<IList<SearchShoppingItemsResponse>>(results);
        }
        finally
        {
            DiagnosticConfig.SearchShoppingItemsDuration.Record(searchStopwatch.Elapsed.TotalSeconds, tags);
        }
    }
}

public sealed record SearchShoppingItemsQuery(Guid UserCode, string SearchText)
    : IQuery<IList<SearchShoppingItemsResponse>>
{
}