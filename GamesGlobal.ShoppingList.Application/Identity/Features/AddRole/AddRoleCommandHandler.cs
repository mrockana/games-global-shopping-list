using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;

public sealed class AddRoleCommandHandler : IApplicationRequestHandler<AddRoleCommand, AddRoleResponse>
{
    private readonly IIdentityRepository _repository;
    private readonly ActivitySource _activitySource;

    public AddRoleCommandHandler(IIdentityRepository repository)
    {
        _repository = repository;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<AddRoleResponse>> Handle(AddRoleCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(AddRoleCommandHandler)}");
        string failedToAdd = "Failed to add role.";

        var findRoleByNameSpec = new FindRoleByName(name: request.RoleName).NoTracking();
        Role? existingRole = await _repository.GetSingleAsync(findRoleByNameSpec, cancellationToken);

        if (existingRole is not null)
        {
            return Result.CreateErrorResult<AddRoleResponse>(new DomainApplicationException($"{failedToAdd} Role with the same name already exists."));
        }

        List<RolePermission> permissionsToAdd = request.Permissions.Select(p =>
        {
            var permissions = (Permissions)p.Value;
            return new RolePermission
            {
                Permission = permissions,
                PermissionName = permissions.GetDescription(),
            };
        }).ToList();

        Role newRole = new()
        {
            Name = request.RoleName,
            RolePermissions = permissionsToAdd,
        };

        Role insertedRole = _repository.Insert<Role>(newRole);

        int saveResults = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResults))
        {
            return Result.CreateErrorResult<AddRoleResponse>(new DomainApplicationException(failedToAdd));
        }

        IList<AddRolePermissionResponse> addRolePermissionsResponse = insertedRole!.RolePermissions?
            .Select(rp => new AddRolePermissionResponse(
                Permission: rp.Permission,
                PermissionDescription: rp.Permission.GetDescription(),
                PermissionDescriptionShort: rp.Permission.ToString()))
            .ToList() ?? new List<AddRolePermissionResponse>();

        AddRoleResponse? tokenResponse = new AddRoleResponse(RoleId: insertedRole.RoleId, Name: insertedRole.Name, Permissions: addRolePermissionsResponse);
        return Result.CreateResult<AddRoleResponse>(tokenResponse);
    }
}

public sealed record AddRoleCommand(string RoleName, IList<AdddRolePermission> Permissions)
    : ICommand<AddRoleResponse>
{
}

public sealed record AdddRolePermission(long Value);
