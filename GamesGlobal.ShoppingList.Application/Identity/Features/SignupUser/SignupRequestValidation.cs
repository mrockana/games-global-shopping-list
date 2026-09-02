using FluentValidation;
using GamesGlobal.ShoppingList.BusinessDomain.Common.StringHelper;
using static GamesGlobal.ShoppingList.Application.Identity.IdentityConstants;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;

public sealed class SignupRequestValidation : AbstractValidator<SignupCommand>
{
    public SignupRequestValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Custom((value, context) =>
            {
                if (!value.IsValidEmail())
                {
                    context.AddFailure(nameof(SignupCommand.Email), "Invalid email format.");
                }
            });

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(ErrorMessages.ConfirmPasswordMustMatchPassword);

        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
    }
}
