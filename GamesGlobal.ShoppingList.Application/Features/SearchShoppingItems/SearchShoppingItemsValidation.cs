using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;

public sealed class SearchShoppingItemsValidation : AbstractValidator<SearchShoppingItemsQuery>
{
    public SearchShoppingItemsValidation()
    {
        RuleFor(r => r.UserCode)
            .NotEmpty()
            .WithMessage($"{nameof(SearchShoppingItemsQuery.UserCode)} is required");

        RuleFor(r => r.SearchText)
                        .NotEmpty()
            .WithMessage($"{nameof(SearchShoppingItemsQuery.SearchText)} is required");
    }
}
