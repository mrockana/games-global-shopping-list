using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;

public sealed class CreateShoppingItemValidation : AbstractValidator<CreateShoppingItemCommandRequest>
{
    public CreateShoppingItemValidation()
    {
        RuleFor(r => r.UserCode).NotEmpty().WithMessage($"{nameof(CreateShoppingItemCommandRequest.UserCode)} is required");
        RuleFor(r => r.Name).NotNull().NotEmpty().WithMessage($"{nameof(CreateShoppingItemCommandRequest.Name)} is required");
        RuleFor(r => r.Description).NotNull().NotEmpty().WithMessage($"{nameof(CreateShoppingItemCommandRequest.Description)} is required");
    }
}
