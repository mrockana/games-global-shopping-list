using System.Globalization;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace GamesGlobal.ShoppingList.xIntegrationTests;

public sealed class GamesGlobalWebApiFactory : WebApplicationFactory<IEndpoint>, IAsyncLifetime
{
    public const string FileObjectStoreBucketName = "my-bucket";

    private const string MinioAccessKey = "minioadmin";
    private const string MinioSecretKey = "minioadmin";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("pgvector/pgvector:pg17")
       .Build();

    private readonly MinioContainer _minio = new MinioBuilder("minio/minio:latest")
       .WithUsername(MinioAccessKey)
       .WithPassword(MinioSecretKey)
       .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private readonly IContainer _ollama = new ContainerBuilder("ollama/ollama:latest")
        .WithPortBinding(11434, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request
            .ForPort(11434)
            .ForPath("/api/tags")))
        .Build();

    private IServiceScope? _serviceScope;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public string FileObjectStoreUrl => string.Create(
        CultureInfo.InvariantCulture,
        $"http://{_minio.Hostname}:{_minio.GetMappedPublicPort(MinioBuilder.MinioPort)}");

    public string OllamaUrl => string.Create(
        CultureInfo.InvariantCulture,
        $"http://{_ollama.Hostname}:{_ollama.GetMappedPublicPort(11434)}");

    public IMinioClient MinioClient
    {
        get
        {
            if (_serviceScope == null)
            {
                _serviceScope = Services.CreateScope();
            }

            return _serviceScope!.ServiceProvider.GetRequiredService<IMinioClient>();
        }
    }

    public IIdentityDbContext IdentityDbContext
    {
        get
        {
            if (_serviceScope == null)
            {
                _serviceScope = Services.CreateScope();
            }

            return _serviceScope!.ServiceProvider.GetRequiredService<IIdentityDbContext>();
        }
    }

    public IApplicationDbContext ApplicationDbContext
    {
        get
        {
            if (_serviceScope == null)
            {
                _serviceScope = Services.CreateScope();
            }

            return _serviceScope!.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        }
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _minio.StartAsync();
        await _redis.StartAsync();
        await _ollama.StartAsync();

        ExecResult pullModelResult = await _ollama.ExecAsync(["ollama", "pull", "embeddinggemma"]);
        if (pullModelResult.ExitCode != 0)
        {
            throw new InvalidOperationException($"Unable to pull the embeddinggemma model: {pullModelResult.Stderr}");
        }
    }

    public new async Task DisposeAsync()
    {
        _serviceScope?.Dispose();
        await base.DisposeAsync();
        await _ollama.DisposeAsync();
        await _redis.DisposeAsync();
        await _minio.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:postgres"] = PostgresConnectionString,
                ["ConnectionStrings:redis"] = RedisConnectionString,
                ["FileObjectStoreOptions:Url"] = FileObjectStoreUrl,
                ["FileObjectStoreOptions:User"] = MinioAccessKey,
                ["FileObjectStoreOptions:Secret"] = MinioSecretKey,
                ["FileObjectStoreOptions:UseSsl"] = "false",
                ["FileObjectStoreOptions:BucketName"] = FileObjectStoreBucketName,
                ["OllamaEmbeddingOptions:Url"] = OllamaUrl,
                ["OllamaEmbeddingOptions:EnableEmbeddingMigrationsTestOnly"] = "true",
            });
        });
    }
}
