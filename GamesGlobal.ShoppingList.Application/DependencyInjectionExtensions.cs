using FluentValidation;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GamesGlobal.ShoppingList.Application;

public static class DependencyInjectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjectionExtensions).Assembly;
        services.Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));
        services.Configure<ShoppingItemsOptions>(configuration.GetSection(nameof(ShoppingItemsOptions)));
        services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IOptionsSnapshot<ShoppingItemsOptions>>().Value);
        services.AddValidatorsFromAssembly(assembly);
        services.AddApplicationRequestProcessor();
    }
}
