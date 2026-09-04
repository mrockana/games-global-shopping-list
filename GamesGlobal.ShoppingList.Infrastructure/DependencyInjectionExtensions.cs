using System;
using System.Linq;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common.Cache;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess.Repository;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Application.SeedData;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Common;
using GamesGlobal.ShoppingList.Infrastructure.DataAccess.Identity;
using GamesGlobal.ShoppingList.Infrastructure.Embeddings;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using StackExchange.Redis;

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
            .UseNpgsql(connectionString, o => o.UseVector())
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
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, DataAccessConstants.IdentitySchema).UseVector())
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

        IConfigurationSection ollamaEmbeddingSection = configuration.GetRequiredSection(nameof(OllamaEmbeddingOptions));
        services.AddOptions<OllamaEmbeddingOptions>()
            .Bind(ollamaEmbeddingSection)
            .Validate(options => Uri.TryCreate(options.Url, UriKind.Absolute, out _), "Url must be a valid absolute URI.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "Model is required.")
            .Validate(options => options.Dimensions > 0, "Dimensions must be greater than zero.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.EmbedEndpoint), "EmbedEndpoint is required.")
            .ValidateOnStart();
        services.AddSingleton(sp => sp.GetRequiredService<IOptions<OllamaEmbeddingOptions>>().Value);

        services.AddHttpClient<IEmbeddingService, OllamaEmbeddingService>((sp, httpClient) =>
        {
            OllamaEmbeddingOptions options = sp.GetRequiredService<OllamaEmbeddingOptions>();
            httpClient.BaseAddress = new Uri(options.Url);
        });
    }

    public static void AddFileObjectStoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection fileObjectStoreSection = configuration.GetRequiredSection(nameof(FileObjectStoreOptions));
        services.Configure<FileObjectStoreOptions>(fileObjectStoreSection);
        services.AddScoped(sp => sp.GetRequiredService<IOptions<FileObjectStoreOptions>>().Value);

        services.AddSingleton<IMinioClient>(sp =>
        {
            FileObjectStoreOptions options = sp.GetRequiredService<IOptions<FileObjectStoreOptions>>().Value;

            return new MinioClient()
                .WithEndpoint(new Uri(options.Url).Authority)
                .WithCredentials(options.User, options.Secret)
                .WithSSL(options.UseSsl)
                .Build();
        });

        services.AddTransient<IFileObjectStoreService, FileObjectStore.FileObjectStoreService>();
    }

    public static void AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        string redisConnectionString = configuration.GetConnectionString("redis") ?? throw new InvalidOperationException("The Redis connection string is required.");
        var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
        redisOptions.AbortOnConnectFail = false;
        redisOptions.ConnectRetry = 1;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisOptions));
        services.AddSingleton<ICacheService, Cache.RedisCacheService>();
    }

    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app)
    {
        await using AsyncServiceScope scope = app.ApplicationServices.CreateAsyncScope();
        await using ApplicationDbContext applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using IdentityDbContext identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DataPersistenceMigration");

        OllamaEmbeddingOptions embeddingOptions = scope.ServiceProvider.GetRequiredService<OllamaEmbeddingOptions>();
        try
        {
            if ((await applicationContext!.Database.GetPendingMigrationsAsync()).Any())
            {
                await applicationContext.Database.MigrateAsync();
            }

            if ((await identityContext.Database.GetPendingMigrationsAsync()).Any())
            {
                await identityContext.Database.MigrateAsync();
            }

            if (embeddingOptions.EnableEmbeddingMigrationsTestOnly)
            {
                await applicationContext.PostMigrationEmbeddingsSeeding(scope.ServiceProvider.GetRequiredService<IEmbeddingService>());
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations");
        }
    }
}
