using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.DataAccess;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace GamesGlobal.ShoppingList.xIntegrationTests;

public sealed class GamesGlobalWebApiFactory : WebApplicationFactory<IEndpoint>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17")
       .Build();
    private IServiceScope? _serviceScope;

    public string PostgresConnectionString => _postgres.GetConnectionString();

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

    public Task InitializeAsync() => _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        _serviceScope?.Dispose();
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ConnectionStrings:postgres"] = PostgresConnectionString,
            });
        });
    }
}
