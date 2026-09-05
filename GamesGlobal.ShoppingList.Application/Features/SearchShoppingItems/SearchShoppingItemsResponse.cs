using System;
using System.Collections.Generic;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;

public sealed record class SearchShoppingItemsResponse(long ShoppingItemId, Guid UserCode, string Name, string? Description, double Distance, double Confidence, IList<SearchShoppingItemsDocumentResponse> Documents)
{
}

public sealed record class SearchShoppingItemsDocumentResponse(long DocumentId, string? MimeType, string? Url, string? Name, int Size)
{
}

public static class SearchShoppingItemsExtensions
{
    public static SearchShoppingItemsDocumentResponse ToSearchShoppingItemsResponse(this Document document)
    {
        return new SearchShoppingItemsDocumentResponse(
            DocumentId: document.DocumentId,
            MimeType: document.MimeType,
            Url: document.Url,
            Size: document.Size,
            Name: document.Name);
    }
}
