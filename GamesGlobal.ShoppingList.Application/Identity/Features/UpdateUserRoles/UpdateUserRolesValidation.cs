using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;

public sealed class UpdateUserRolesValidation : AbstractValidator<UpdateUserRolesCommand>
{
    public UpdateUserRolesValidation()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .NotEmpty<UpdateUserRolesCommand, long>();
    }
}
