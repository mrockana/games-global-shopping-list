using FluentValidation;
using GamesGlobal.ShoppingList.BusinessDomain.Common.StringHelper;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.Login;

public sealed class LoginValidation : AbstractValidator<SessionLoginCommand>
{
    public LoginValidation()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Custom((value, context) =>
            {
                if (!value.IsValidEmail())
                {
                    context.AddFailure(nameof(SessionLoginCommand.Username), "Invalid email format.");
                }
            });
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
    }
}
