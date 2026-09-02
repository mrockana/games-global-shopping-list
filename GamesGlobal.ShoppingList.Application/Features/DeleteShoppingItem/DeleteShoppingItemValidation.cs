using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;

public sealed class DeleteShoppingItemValidation : AbstractValidator<DeleteShoppingItemCommand>
{
    public DeleteShoppingItemValidation()
    {
        RuleFor(r => r.ShoppingItemId)
            .NotEmpty().WithMessage($"{nameof(DeleteShoppingItemCommand.ShoppingItemId)} is required");
    }
}
