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
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.Login;

public sealed class LoginCommandHandler : IApplicationRequestHandler<SessionLoginCommand, LoginResponse>
{
    private readonly IIdentityRepository _repository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenService _loginSessionCreator;
    private readonly IdentityModuleOptions _identityModuleOptions;
    private readonly ActivitySource _activitySource;

    public LoginCommandHandler(IIdentityRepository repository, IJwtTokenGenerator jwtTokenGenerator, IOptions<IdentityModuleOptions> identityModuleOptions, IRefreshTokenService loginSessionCreator)
    {
        _repository = repository;
        _jwtTokenGenerator = jwtTokenGenerator;
        _identityModuleOptions = identityModuleOptions.Value;
        _activitySource = DiagnosticConfig.ActivitySource;
        _loginSessionCreator = loginSessionCreator;
    }

    public async Task<Result<LoginResponse>> Handle(SessionLoginCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(LoginCommandHandler)}");
        FindUserByEmail findUserSpecification = new(request.Username);

        findUserSpecification
            .WithQuery(q => q.Include(u => u.Roles)
            .ThenInclude(r => r.RolePermissions))
            .NoTracking();

        User? user = await _repository.GetSingleAsync(findUserSpecification, cancellationToken);
        string loginFailMessage = "Incorrect credentials.";

        if (user is null)
        {
            return Result.CreateErrorResult<LoginResponse>(new DomainUnauthorizedAccessException(loginFailMessage));
        }

        if (!user.VerifyUserPassword(request.Password))
        {
            return Result.CreateErrorResult<LoginResponse>(new DomainUnauthorizedAccessException(loginFailMessage));
        }

        (BusinessDomain.Identity.Entities.RefreshToken? loginSession, bool sessionCreatedSuccessful) = await _loginSessionCreator.CreateRefreshToken(_repository, user);

        if (!sessionCreatedSuccessful)
        {
            return Result.CreateErrorResult<LoginResponse>(new DomainApplicationException("Failed to create login session"));
        }

        Permissions permissions = PermissionPolicyHelper.GetPermissionsFrom(user!.Roles.ToList());
        string token = _jwtTokenGenerator.Generate(user.Email!, user.UserCode, permissions);

        LoginResponse response = new(Token: token,
            ExpiresInMinutes: _identityModuleOptions.JwtExpiresInMinutes,
            RefreshToken: loginSession!.Token,
            RefreshTokenExpiresInMinutes: _identityModuleOptions.RefreshTokenExpiresInMinutes,
            Permissions: permissions);

        return Result.CreateResult<LoginResponse>(response);
    }
}

public sealed record SessionLoginCommand(string Username, string Password)
    : ICommand<LoginResponse>
{
}
