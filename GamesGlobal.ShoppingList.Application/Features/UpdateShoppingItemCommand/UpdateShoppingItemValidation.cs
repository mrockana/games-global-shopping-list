using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;

public sealed class UpdateShoppingItemValidation : AbstractValidator<UpdateShoppingItemCommandRequest>
{
    public UpdateShoppingItemValidation()
    {
        RuleFor(r => r.ShoppingItemId).NotNull().NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.ShoppingItemId)} is required");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.Name)} is required");
        RuleFor(r => r.Description).NotNull().NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.Description)} is required");
    }
}
