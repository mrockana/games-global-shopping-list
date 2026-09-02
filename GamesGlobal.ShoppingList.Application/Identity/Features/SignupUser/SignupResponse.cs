using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;

namespace GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;

public sealed record SignupResponse
(string FirstName,
    string LastName,
    string Email,
    long UserId);

public static class SignupResponseExtensions
{
    public static SignupResponse ToRegisterUserResponse(this User entity)
    {
        return new SignupResponse(
            entity.FirstName!,
            entity.LastName!,
            entity.Email!,
            entity.UserId);
    }
}

public static class SignupCommandExtensions
{
    public static User ToEntity(this SignupCommand request)
    {
        return new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email?.ToLowerInvariant(),
        };
    }
}