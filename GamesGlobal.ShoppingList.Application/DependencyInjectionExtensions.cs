using FluentValidation;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GamesGlobal.ShoppingList.Application;

public static class DependencyInjectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjectionExtensions).Assembly;
        services.Configure<CacheOptions>(configuration.GetSection(nameof(CacheOptions)));
        services.AddValidatorsFromAssembly(assembly);
        services.AddApplicationRequestProcessor();
    }
}
