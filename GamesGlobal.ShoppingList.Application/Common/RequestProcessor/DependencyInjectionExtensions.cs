using Microsoft.Extensions.DependencyInjection;

namespace GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

public static class DependencyInjectionExtensions
{
    internal static IServiceCollection AddApplicationRequestProcessor(this IServiceCollection services)
    {
        services.AddTransient<ApplicationRequestProcessor>();
        return services.Scan(scan => scan
             .FromAssemblyOf<IApplicationRequest<BaseResult>>() // Scan the Application assembly
             .AddClasses(classes => classes
                 .AssignableTo(typeof(IApplicationRequestHandler<,>)))
             .AsImplementedInterfaces()
             .WithTransientLifetime());
    }
}
