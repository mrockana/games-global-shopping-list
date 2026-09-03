using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;

public sealed class UpdateShoppingItemValidation : AbstractValidator<UpdateShoppingItemCommandRequest>
{
    public UpdateShoppingItemValidation()
    {
        RuleFor(r => r.ShoppingItemId).GreaterThan(0).WithMessage($"{nameof(UpdateShoppingItemCommandRequest.ShoppingItemId)} must be greater than zero");
        RuleFor(r => r.UserCode).NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.UserCode)} is required");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.Name)} is required");
        RuleFor(r => r.Description).NotNull().NotEmpty().WithMessage($"{nameof(UpdateShoppingItemCommandRequest.Description)} is required");
    }
}
