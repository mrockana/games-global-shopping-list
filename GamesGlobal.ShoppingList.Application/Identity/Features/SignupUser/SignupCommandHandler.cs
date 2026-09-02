using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;

public sealed class SignupCommandHandler : IApplicationRequestHandler<SignupCommand, SignupResponse>
{
    private readonly IIdentityRepository _repository;
    private readonly ActivitySource _activitySource;

    public SignupCommandHandler(IIdentityRepository repository)
    {
        _repository = repository;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<SignupResponse>> Handle(SignupCommand request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity($"Running {nameof(SignupCommandHandler)}");
        FindUserByEmail findUserSpecification = new FindUserByEmail(request.Email);
        User? user = await _repository.GetSingleAsync(findUserSpecification, cancellationToken);

        if (user is not null)
        {
            return Result.CreateErrorResult<SignupResponse>(new DomainValidationException("User already exists"));
        }

        User userEntity = request.ToEntity();

        userEntity.SetUserPassword(password: request.Password);

        User? insertedUser = _repository.Insert(userEntity);

        var saveResult = await _repository.SaveAsync(cancellationToken);

        if (!_repository.SavedSuccessful(saveResult))
        {
            return Result.CreateErrorResult<SignupResponse>(new DomainApplicationException("Failed to save user"));
        }

        return Result.CreateResult<SignupResponse>(insertedUser.ToRegisterUserResponse());
    }
}

public sealed record SignupCommand
(string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword)
    : ICommand<SignupResponse>
{
}