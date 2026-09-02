using FluentValidation;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;

public sealed class UpdateRolePermissionsValidation : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsValidation()
    {
        RuleFor(x => x.RoleId)
            .GreaterThan(0)
            .NotEmpty<UpdateRolePermissionsCommand, long>();

        RuleFor(x => x.Permissions)
            .NotEmpty();
    }
}
