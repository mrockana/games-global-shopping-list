using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;

public sealed class RefreshTokenValidation : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidation()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.UserCode)
            .NotEmpty()
            .WithMessage($"{nameof(RefreshTokenCommand.UserCode)} is required");
    }
}
