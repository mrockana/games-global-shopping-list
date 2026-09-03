using GamesGlobal.ShoppingList.BusinessDomain.Entities;

namespace GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;

public sealed record class UploadShoppingItemImageResponse(long DocumentId, long ShoppingItemId, string Name, string MimeType, int Size, string Url)
{
}

public static class UploadShoppingItemImageResponseExtensions
{
    public static UploadShoppingItemImageResponse ToUploadShoppingItemImageResponse(this Document entity, long shoppingItemId)
    {
        return new UploadShoppingItemImageResponse(
            DocumentId: entity.DocumentId,
            ShoppingItemId: shoppingItemId,
            Name: entity.Name!,
            MimeType: entity.MimeType!,
            Size: entity.Size,
            Url: entity.Url!);
    }
}
