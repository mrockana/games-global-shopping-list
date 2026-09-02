using System;
using GamesGlobal.ShoppingList.BusinessDomain.Identity;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.TokenGenerators;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Features.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamesGlobal.ShoppingList.BusinessDomain;

public static class DependencyInjectionExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MA0025:Implement the functionality instead of throwing NotImplementedException", Justification = "Please remove this")]
    public static void AddBusinessDomainServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Identity Feature
        IConfigurationSection? identityOptionsSection = configuration.GetRequiredSection(nameof(IdentityModuleOptions));
        services.Configure<IdentityModuleOptions>(identityOptionsSection);
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddTransient<IUserHashGenerator, UserHashGenerator>();
        services.AddTransient<IRefreshTokenService, RefreshTokenService>();
    }
}