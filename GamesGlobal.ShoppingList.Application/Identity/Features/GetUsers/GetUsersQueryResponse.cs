using System.Collections.Generic;
using System.Linq;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.GetUsers;

public sealed record GetUsersQueryResponse(
    long UserId,
    string FirstName,
    string LastName,
    string Email,
    IList<GetUsersRoleResponse> Roles);

public sealed record GetUsersRoleResponse(long RoleId, string Name);

public static class GetUsersQueryResponseExtensions
{
    public static GetUsersQueryResponse ToGetUsersQueryResponse(this User user)
    {
        IList<GetUsersRoleResponse> roles = user.Roles?
            .Select(r => new GetUsersRoleResponse(r.RoleId, r.Name))
            .ToList() ?? new List<GetUsersRoleResponse>();

        return new GetUsersQueryResponse(
            UserId: user.UserId,
            FirstName: user.FirstName ?? string.Empty,
            LastName: user.LastName ?? string.Empty,
            Email: user.Email ?? string.Empty,
            Roles: roles);
    }
}