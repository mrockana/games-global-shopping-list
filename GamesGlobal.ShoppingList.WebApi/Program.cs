using GamesGlobal.ShoppingList.Application;
using GamesGlobal.ShoppingList.BusinessDomain;
using GamesGlobal.ShoppingList.Infrastructure;
using GamesGlobal.ShoppingList.WebApi;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
IConfiguration configuration = builder.Configuration;

builder.Host.ConfigureSerilog(configuration, builder.Environment.IsDevelopment());

builder.Logging.SetupOpenTelemetryLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

#region Add Endpoints
builder.Services.AddEndpoints();
#endregion

builder.Services.AddHttpLogging();

// Add services to the container.
builder.Services.AddOpenTelemetryServices(configuration, builder.Environment.IsDevelopment());
builder.Services.AddDataInfrastructureServices(configuration, builder.Environment.IsDevelopment());
builder.Services.AddProblemDetails();
builder.Services.AddApplicationServices(configuration);
builder.Services.AddBusinessDomainServices(configuration);
builder.Services.AddIdentityAuth(configuration);
builder.Services.AddTransient<NonSuccessResponseMiddleware>();
builder.Services.SetupRateLimiter(configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<NonSuccessResponseMiddleware>();
app.UseHttpLogging();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapOpenApi();
app.MapScalarApiReference("/docs");
app.UseHttpsRedirection();

#region Add Endpoints
app.MapEndpoints();
if (app.Environment.IsDevelopment())
{
    app.ApplyMigrations();
}

#endregion

await app.RunAsync();