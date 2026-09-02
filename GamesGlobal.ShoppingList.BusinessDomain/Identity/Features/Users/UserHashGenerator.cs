using System;
using System.Text;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Entities;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.TokenGenerators;

public sealed class UserHashGenerator : IUserHashGenerator
{
    private readonly IdentityModuleOptions _identityModuleOptions;

    public UserHashGenerator(IOptions<IdentityModuleOptions> identityModuleOptions)
    {
        _identityModuleOptions = identityModuleOptions.Value;
    }

    public string GenerateHashedToken(User user)
    {
        Guid randomGuid = Guid.NewGuid();
        string unhashedToken = $"{randomGuid.ToString()}-{_identityModuleOptions.HashedTokenSigningKey}";
        string hashedToken = new PasswordHasher<User>().HashPassword(user, unhashedToken);
        byte[]? byteArrayToken = Encoding.UTF8.GetBytes(hashedToken);

        return Convert.ToBase64String(byteArrayToken);
    }
}
