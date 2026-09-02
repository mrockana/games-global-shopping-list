using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;

public sealed class AddRoleCommandValidation : AbstractValidator<AddRoleCommand>
{
    public AddRoleCommandValidation()
    {
        RuleFor(x => x.RoleName)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.Permissions)
            .NotEmpty();
    }
}
