using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImageCommandCommand;

public sealed class UploadShoppingItemImageCommandValidation : AbstractValidator<UploadShoppingItemImageCommandCommandRequest>
{
    public UploadShoppingItemImageCommandValidation()
    {
        RuleFor(r => r.UploadShoppingItemImageCommandId).NotNull().NotEmpty().WithMessage($"{nameof(UploadShoppingItemImageCommandCommandRequest.UploadShoppingItemImageCommandId)} is required");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage($"{nameof(UploadShoppingItemImageCommandCommandRequest.Name)} is required");
    }
}
