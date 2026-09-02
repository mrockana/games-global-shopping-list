using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Roles;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler : IApplicationRequestHandler<UpdateUserRolesCommand, UpdateUserRolesResponse>
{
    private readonly IIdentityRepository _repository;
    private readonly ILogger<UpdateUserRolesCommandHandler> _logger;
    private readonly ActivitySource _activitySource;

    public UpdateUserRolesCommandHandler(IIdentityRepository repository, ILogger<UpdateUserRolesCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<UpdateUserRolesResponse>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(UpdateUserRolesCommandHandler)}");

        var findRoleByIdSpec = new FindUserById(request.UserId)
            .Include(r => r.Roles);

        User? user = await _repository.GetSingleAsync(findRoleByIdSpec, cancellationToken);
        string failedToUpdate = "Failed to update user roles.";
        string notFound = "User Not Found";

        if (user is null)
        {
            return Result.CreateErrorResult<UpdateUserRolesResponse>(new DomainApplicationException(notFound));
        }

        IList<long> roleIds = request.RoleIds ?? new List<long>();

        IList<Role> rolesToRemove = user.Roles?
            .Where(role => !roleIds.Any(p => p == role.RoleId))
            .ToList() ?? new List<Role>();

        List<long> roleIdsToAdd = roleIds
             .Where(rId => !user.Roles?.Any(role => role.RoleId == rId) ?? false)
             .ToList();

        foreach (var roleId in roleIdsToAdd)
        {
            var roleToAddSpec = new FindRoleById(roleId);

            Role? roleToAdd = await _repository.GetSingleAsync(roleToAddSpec, cancellationToken);

            if (roleToAdd is not null)
            {
                _logger.LogInformation("Adding new User Role: {RoleId}", roleId);
                user.Roles!.Add(roleToAdd);
            }
        }

        foreach (var roleToRemove in rolesToRemove)
        {
            _logger.LogInformation("Removed the following Role {RoleId} for User {UserId}", roleToRemove.RoleId, user.UserId);
            user.Roles!.Remove(roleToRemove);
        }

        int saveResults = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResults))
        {
            return Result.CreateErrorResult<UpdateUserRolesResponse>(new DomainApplicationException(failedToUpdate));
        }

        IList<UpdateUserRoleRoleResponse> roles = user!.Roles?
            .Select(r => new UpdateUserRoleRoleResponse(
                RoleId: r.RoleId,
                Name: r.Name))
            .ToList() ?? new List<UpdateUserRoleRoleResponse>();

        UpdateUserRolesResponse? tokenResponse = new UpdateUserRolesResponse(UserId: user.UserId, Roles: roles);
        return Result.CreateResult<UpdateUserRolesResponse>(tokenResponse);
    }
}

public sealed record UpdateUserRolesCommand(long UserId, IList<long>? RoleIds)
    : ICommand<UpdateUserRolesResponse>
{
}
