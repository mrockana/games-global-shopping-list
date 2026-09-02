using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using GamesGlobal.ShoppingList.BusinessDomain.Features;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.GetRoles;

public sealed class GetRolesQueryHandler : IApplicationRequestHandler<GetRolesQuery, IList<GetRolesQueryResponse>>
{
    private readonly IIdentityRepository _repository;
    private readonly ActivitySource _activitySource;

    public GetRolesQueryHandler(IIdentityRepository repository)
    {
        _repository = repository;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<IList<GetRolesQueryResponse>>> Handle(GetRolesQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(GetRolesQueryHandler)}");
        var findAllSpec = new FindAll<Role>()
            .Include(r => r.RolePermissions)
            .NoTracking();

        IList<Role> roles = await _repository.GetAsync(findAllSpec, cancellationToken);

        List<Permissions> availablePermissions = Enum.GetValues(typeof(Permissions))
                    .Cast<Permissions>()
                    .ToList();

        IList<GetRolesQueryResponse> rolesResponse = roles.Select(r =>
        {
            return new GetRolesQueryResponse(
                RoleId: r.RoleId,
                Name: r.Name,
                Permissions: GetPermissionsForRole(r, availablePermissions));
        }).ToList();

        return Result.CreateResult<IList<GetRolesQueryResponse>>(rolesResponse);
    }

    private IList<PermissionResponse> GetPermissionsForRole(Role role, List<Permissions> availablePermissions)
    {
        return availablePermissions.Select(selector: p =>
        {
            bool enabled = role.RolePermissions?.Any(rp => rp.Permission == p) ?? false;
            return new PermissionResponse(
                Permission: p,
                PermissionDescriptionShort: p.ToString(),
                PermissionDescription: p.GetDescription(),
                Enabled: enabled);
        }).ToList();
    }
}

public sealed record GetRolesQuery(string message = "Get Roles")
    : IQuery<IList<GetRolesQueryResponse>>
{
}
