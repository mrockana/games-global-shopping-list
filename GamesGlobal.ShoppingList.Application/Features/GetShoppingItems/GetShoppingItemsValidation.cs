using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;

public sealed class GetShoppingItemsValidation : AbstractValidator<GetShoppingItemsQuery>
{
    public GetShoppingItemsValidation()
    {
        RuleFor(r => r.UserCode)
            .NotEmpty()
            .WithMessage($"{nameof(GetShoppingItemsQuery.UserCode)} is required");
    }
}
