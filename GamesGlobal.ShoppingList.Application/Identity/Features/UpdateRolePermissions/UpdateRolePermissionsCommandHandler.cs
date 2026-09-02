using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Common.StringHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Roles;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;

public sealed class UpdateRolePermissionsCommandHandler : IApplicationRequestHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsResponse>
{
    private readonly IIdentityRepository _repository;
    private readonly ILogger<UpdateRolePermissionsCommandHandler> _logger;
    private readonly ActivitySource _activitySource;

    public UpdateRolePermissionsCommandHandler(IIdentityRepository repository, ILogger<UpdateRolePermissionsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<UpdateRolePermissionsResponse>> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(UpdateRolePermissionsCommandHandler)}");

        var findRoleByIdSpec = new FindRoleById(request.RoleId)
            .Include(r => r.RolePermissions);

        Role? role = await _repository.GetSingleAsync(findRoleByIdSpec, cancellationToken);
        string failedToUpdate = "Failed to update Role Permissions.";
        string notFound = "Role Not Found";

        if (role is null)
        {
            return Result.CreateErrorResult<UpdateRolePermissionsResponse>(new DomainNotFoundException(notFound));
        }

        if (!request.RoleName.IsNullOrWhiteSpace() &&
            !request.RoleName.Equals(role.Name, System.StringComparison.InvariantCulture))
        {
            _logger.LogInformation("Updating Role Name from {OldName} to {NewName}", role.Name, request.RoleName);
            role.Name = request.RoleName;
        }

        IList<RolePermission> permissionsToRemove = role.RolePermissions?
            .Where(rp => !request.Permissions.Any(p => p.Value == (long)rp.Permission))
            .ToList() ?? new List<RolePermission>();

        List<Permission> permissionsToAdd = request.Permissions?
             .Where(p => !role.RolePermissions?.Any(rp => (long)rp.Permission == p.Value) ?? false)
             .ToList() ?? new List<Permission>();

        foreach (var permission in permissionsToAdd)
        {
            var permissions = (Permissions)permission.Value;
            RolePermission newRolePermission = new RolePermission { RoleId = role.RoleId, Permission = permissions, PermissionName = permissions.GetDescription(), };

            _logger.LogInformation("Added new Role Permission: {PermissionName}", permissions.GetDescription());
            role.RolePermissions!.Add(newRolePermission);
        }

        foreach (var permissionToRemove in permissionsToRemove)
        {
            _logger.LogInformation("Removed the following Role Permissions: {RolePermissionId}", permissionToRemove.RolePermissionId);
            role.RolePermissions!.Remove(permissionToRemove);
        }

        int saveResults = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResults))
        {
            return Result.CreateErrorResult<UpdateRolePermissionsResponse>(new DomainApplicationException(failedToUpdate));
        }

        IList<UpdateRolePermission> updateRolePermissionsResponse = role!.RolePermissions?
            .Select(rp => new UpdateRolePermission(
                Permission: rp.Permission,
                PermissionDescription: rp.Permission.GetDescription(),
                PermissionDescriptionShort: rp.Permission.ToString()))
            .ToList() ?? new List<UpdateRolePermission>();

        UpdateRolePermissionsResponse? tokenResponse = new UpdateRolePermissionsResponse(RoleId: role.RoleId, Name: role.Name, Permissions: updateRolePermissionsResponse);
        return Result.CreateResult<UpdateRolePermissionsResponse>(tokenResponse);
    }
}

public sealed record UpdateRolePermissionsCommand(long RoleId, string RoleName, IList<Permission> Permissions)
    : ICommand<UpdateRolePermissionsResponse>
{
}

public sealed record Permission(long Value);
