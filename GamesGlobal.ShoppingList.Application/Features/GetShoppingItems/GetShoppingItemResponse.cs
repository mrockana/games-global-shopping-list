using System;
using System.Collections.Generic;
using System.Linq;
using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;

public sealed record class GetShoppingItemResponse(long ShoppingItemId, Guid UserCode, string Name, string? Description, IList<GetShoppingItemDocumentResponse> Documents)
{
}

public sealed record class GetShoppingItemDocumentResponse(long DocumentId, string? MimeType, string? Url, string? Name, int Size)
{
}

public static class GetShoppingItemResponseExtensions
{
    public static GetShoppingItemDocumentResponse ToGetShoppingItemDocumentResponse(this Document document)
    {
        return new GetShoppingItemDocumentResponse(
            DocumentId: document.DocumentId,
            MimeType: document.MimeType,
            Url: document.Url,
            Size: document.Size,
            Name: document.Name);
    }

    public static GetShoppingItemResponse ToGetShoppingItemResponse(this ShoppingItem entity)
    {
        return new GetShoppingItemResponse(
            UserCode: entity.UserCode,
            ShoppingItemId: entity.ShoppingItemId,
            Name: entity.Name!,
            Description: entity.Description,
            Documents: entity.Documents?.Select(d => d.ToGetShoppingItemDocumentResponse()).ToList() ?? new List<GetShoppingItemDocumentResponse>());
    }
}
