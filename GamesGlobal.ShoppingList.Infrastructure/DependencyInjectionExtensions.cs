using System;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Common;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static void AddDataInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = true)
    {
        services.AddScoped<AuditFieldsModificationInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(DataAccessConstants.PostgresConnectionStringName);
            options
            .UseNpgsql(connectionString)
            .AddInterceptors(sp.GetRequiredService<AuditFieldsModificationInterceptor>());

            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString(DataAccessConstants.PostgresConnectionStringName);
            options
            .UseNpgsql(
                connectionString,
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, DataAccessConstants.IdentitySchema))
            .AddInterceptors(sp.GetRequiredService<AuditFieldsModificationInterceptor>());

            if (isDevelopment)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        services.AddTransient<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<IdentityDbContext>());
        services.AddTransient<IIdentityRepository, IdentityRepository>();
    }

    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        using IdentityDbContext identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataPersistenceMigration");

        try
        {
            applicationContext.Database.Migrate();
            identityContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations");
        }
    }
}
