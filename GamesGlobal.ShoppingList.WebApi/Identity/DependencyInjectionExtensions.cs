using System.Text;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GamesGlobal.ShoppingList.WebApi.Identity;

internal static class DependencyInjectionExtensions
{
    internal static void AddIdentityAuth(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection? identityOptionsSection = configuration.GetRequiredSection(nameof(IdentityModuleOptions));
        IdentityModuleOptions? identityOptions = identityOptionsSection.Get<IdentityModuleOptions>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = identityOptions!.ApplicationUrl,
                    ValidAudience = identityOptions!.ApplicationUrl,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityOptions.JwtSigningKey)),
                };
            })
            .AddJwtBearer("RefreshToken", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = identityOptions!.ApplicationUrl,
                    ValidAudience = identityOptions!.ApplicationUrl,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(identityOptions.JwtSigningKey)),
                };
            });

        services.AddAuthorization();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();
    }
}
