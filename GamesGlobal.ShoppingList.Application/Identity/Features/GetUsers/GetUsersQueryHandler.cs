using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Pagination;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Features;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.GetUsers;

public sealed class GetUsersQueryHandler : IApplicationRequestHandler<GetUsersQuery, PaginatedResults<IList<GetUsersQueryResponse>>>
{
    private readonly IIdentityRepository _repository;
    private readonly ActivitySource _activitySource;

    public GetUsersQueryHandler(IIdentityRepository repository)
    {
        _repository = repository;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<PaginatedResults<IList<GetUsersQueryResponse>>>> Handle(GetUsersQuery request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(GetUsersQueryHandler)}");

        var findAllSpec = new FindAll<User>()
            .NoTracking();

        var totalUsers = await _repository.CountAsync(findAllSpec, cancellationToken);

        if (totalUsers == 0)
        {
            var notFoundEx = new DomainNotFoundException("No users found.");
            activity?.AddException(notFoundEx, new TagList { { "Message", notFoundEx.Message } });
            return Result.CreateErrorResult<PaginatedResults<IList<GetUsersQueryResponse>>>(notFoundEx);
        }

        var paginatedSpec = findAllSpec
            .Include(u => u.Roles)
            .WithPagination(request.Take, request.Skip);

        IList<User> users = (await _repository.GetAsync(paginatedSpec, cancellationToken)) ?? Enumerable.Empty<User>().ToList();

        IList<GetUsersQueryResponse> usersResponse = users
            .Select(u => u.ToGetUsersQueryResponse())
            .ToList();

        var response = new PaginatedResults<IList<GetUsersQueryResponse>>(
            Data: usersResponse,
            TotalRecords: totalUsers,
            PageSize: request.Take,
            TotalPages: (int)Math.Ceiling((double)totalUsers / request.Take),
            CurrentPage: (request.Skip / request.Take) + 1);

        return Result.CreateResult<PaginatedResults<IList<GetUsersQueryResponse>>>(response);
    }
}

public sealed record GetUsersQuery(int Take = 10, int Skip = 0) : PaginatedRequestBase(Take, Skip), IQuery<PaginatedResults<IList<GetUsersQueryResponse>>>
{
}