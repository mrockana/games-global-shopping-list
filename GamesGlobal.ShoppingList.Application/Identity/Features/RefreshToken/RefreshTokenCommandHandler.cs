using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;

public sealed class RefreshTokenCommandHandler : IApplicationRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserHashGenerator _hashedTokenGenerator;
    private readonly IdentityModuleOptions _identityModuleOptions;
    private readonly ActivitySource _activitySource;

    public RefreshTokenCommandHandler(IIdentityRepository repository, IJwtTokenGenerator jwtTokenGenerator, IOptions<IdentityModuleOptions> identityModuleOptions, IUserHashGenerator hashedTokenGenerator)
    {
        _identityRepository = repository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityModuleOptions = identityModuleOptions.Value;
        _activitySource = DiagnosticConfig.ActivitySource;
        _hashedTokenGenerator = hashedTokenGenerator;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(RefreshTokenCommandHandler)}");

        var findRefreshToken = new FindActiveRefreshTokenByToken(request.RefreshToken);
        BusinessDomain.Identity.Entities.RefreshToken? loginSession = await _identityRepository.GetSingleAsync(findRefreshToken, cancellationToken);

        var forbiddenActionMessage = "Forbidden Action: Session invalid/expired.";

        if (loginSession is null)
        {
            return Result.CreateErrorResult<RefreshTokenResponse>(new DomainForbiddenActionException(forbiddenActionMessage));
        }

        var userSpec = new FindUserByUserCode(request.UserCode)
            .AsSplitQuery()
            .WithQuery(q => q.Include(u => u.Roles)
            .ThenInclude(r => r.RolePermissions))
            .NoTracking();

        User? user = await _identityRepository.GetSingleAsync(userSpec, cancellationToken);

        if (user is null || user.UserId != loginSession.UserId)
        {
            return Result.CreateErrorResult<RefreshTokenResponse>(new DomainForbiddenActionException(forbiddenActionMessage));
        }

        string newRefreshToken = _hashedTokenGenerator.GenerateHashedToken(user);
        loginSession.ExpiryDate = DateTime.UtcNow.AddMinutes(_identityModuleOptions.RefreshTokenExpiresInMinutes);
        loginSession.Token = newRefreshToken;

        int saveResults = await _identityRepository.SaveAsync(cancellationToken);

        if (!_identityRepository.SavedSuccessful(saveResults))
        {
            return Result.CreateErrorResult<RefreshTokenResponse>(new DomainApplicationException("Failed to refresh token."));
        }

        Permissions permissions = PermissionPolicyHelper.GetPermissionsFrom(user!.Roles.ToList());

        string? token = _jwtTokenGenerator.Generate(user.Email!, user.UserCode, permissions);

        RefreshTokenResponse tokenResponse = new(Token: token,
            ExpiresInMinutes: _identityModuleOptions.JwtExpiresInMinutes,
            RefreshToken: loginSession!.Token,
            RefreshTokenExpiresInMinutes: _identityModuleOptions.RefreshTokenExpiresInMinutes,
            Permissions: permissions);

        return Result.CreateResult<RefreshTokenResponse>(tokenResponse);
    }
}

public sealed record RefreshTokenCommand(string RefreshToken, Guid UserCode)
    : ICommand<RefreshTokenResponse>
{
}
