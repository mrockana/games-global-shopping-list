using System;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IdentityModuleOptions _identityModuleOptions;
    private readonly IUserHashGenerator _hashedTokenGenerator;

    public RefreshTokenService(IOptions<IdentityModuleOptions> identityModuleOptions, IUserHashGenerator hashedTokenGenerator)
    {
        _identityModuleOptions = identityModuleOptions.Value;
        _hashedTokenGenerator = hashedTokenGenerator;
    }

    public async Task<(Entities.RefreshToken? newSession, bool isSuccess)> CreateRefreshToken(IIdentityRepository identityRepository, User user)
    {
        string refreshToken = _hashedTokenGenerator.GenerateHashedToken(user);

        Entities.RefreshToken loginSession = new()
        {
            Token = refreshToken,
            ExpiryDate = DateTime.UtcNow.AddMinutes(_identityModuleOptions.RefreshTokenExpiresInMinutes),
            UserId = user.UserId,
        };

        Entities.RefreshToken insertedSession = identityRepository.Insert(loginSession);
        int saveResult = await identityRepository.SaveAsync();

        return (insertedSession, identityRepository.SavedSuccessful(saveResult));
    }
}
