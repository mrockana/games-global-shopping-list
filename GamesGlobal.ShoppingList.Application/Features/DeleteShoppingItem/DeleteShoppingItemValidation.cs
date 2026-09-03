using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;

public sealed class DeleteShoppingItemValidation : AbstractValidator<DeleteShoppingItemCommand>
{
    public DeleteShoppingItemValidation()
    {
        RuleFor(r => r.ShoppingItemId)
            .GreaterThan(0).WithMessage($"{nameof(DeleteShoppingItemCommand.ShoppingItemId)} must be greater than zero");
    }
}
