using System.Globalization;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

    private FakeOllamaServer? _ollama;
    private IServiceScope? _serviceScope;

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public string RedisConnectionString => _redis.GetConnectionString();

    public string FileObjectStoreUrl => string.Create(
        CultureInfo.InvariantCulture,
        $"http://{_minio.Hostname}:{_minio.GetMappedPublicPort(MinioBuilder.MinioPort)}");

    public string OllamaUrl => _ollama?.Url ?? throw new InvalidOperationException("The fake Ollama server has not started.");

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
        _ollama = await FakeOllamaServer.StartAsync();
        await _redis.StartAsync();
        await _postgres.StartAsync();
        await _minio.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        _serviceScope?.Dispose();
        await base.DisposeAsync();
        if (_ollama is not null)
        {
            await _ollama.DisposeAsync();
        }

        await _minio.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:redis"] = RedisConnectionString,
                ["ConnectionStrings:postgres"] = PostgresConnectionString,
                ["OllamaEmbeddingOptions:Url"] = OllamaUrl,
                ["FileObjectStoreOptions:Url"] = FileObjectStoreUrl,
                ["FileObjectStoreOptions:User"] = MinioAccessKey,
                ["FileObjectStoreOptions:Secret"] = MinioSecretKey,
                ["FileObjectStoreOptions:UseSsl"] = "false",
                ["FileObjectStoreOptions:BucketName"] = FileObjectStoreBucketName,
            });
        });
    }

    private sealed class FakeOllamaServer : IAsyncDisposable
    {
        private const int EmbeddingDimensions = 768;
        private readonly WebApplication _application;

        private FakeOllamaServer(WebApplication application)
        {
            _application = application;
        }

        public string Url => _application.Urls.Single();

        public static async Task<FakeOllamaServer> StartAsync()
        {
            WebApplicationBuilder builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            WebApplication application = builder.Build();

            application.MapPost("/api/embed", (EmbedRequest request) => Results.Ok(new
            {
                embeddings = request.Input.Select(_ => new float[EmbeddingDimensions]).ToArray(),
            }));

            await application.StartAsync(application.Lifetime.ApplicationStarted);
            return new FakeOllamaServer(application);
        }

        public ValueTask DisposeAsync() => _application.DisposeAsync();

        private sealed record EmbedRequest(IReadOnlyList<string> Input);
    }
}
