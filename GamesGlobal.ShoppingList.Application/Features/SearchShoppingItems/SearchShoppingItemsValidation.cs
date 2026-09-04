using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;

public sealed class SearchShoppingItemsValidation : AbstractValidator<SearchShoppingItemsQuery>
{
    public SearchShoppingItemsValidation()
    {
        RuleFor(r => r.UserCode)
            .NotEmpty()
            .WithMessage($"{nameof(SearchShoppingItemsQuery.UserCode)} is required");

        RuleFor(r => r.Search)
                        .NotEmpty()
            .WithMessage($"{nameof(SearchShoppingItemsQuery.Search)} is required");
    }
}
