using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IdentityModuleOptions _identityModuleOptions;

    public JwtTokenGenerator(IOptions<IdentityModuleOptions> identityModuleOptions)
    {
        _identityModuleOptions = identityModuleOptions.Value;
    }

    public string Generate(string username, Guid userCode, Permissions permissions)
    {
        long permissionsLong = (long)permissions;

        Claim[] claims = new[]
        {
            new Claim(IdentityDomainConstants.PermissionsClaimName, $"{permissionsLong.ToString()}"),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Email, username),
            new Claim(IdentityDomainConstants.UserCodeClaimName, userCode.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_identityModuleOptions.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _identityModuleOptions.ApplicationUrl,
            audience: _identityModuleOptions.ApplicationUrl,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_identityModuleOptions.JwtExpiresInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
