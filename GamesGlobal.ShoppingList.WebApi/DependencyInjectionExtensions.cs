using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using CommomConstants = GamesGlobal.ShoppingList.Application.Common.Constants;

namespace GamesGlobal.ShoppingList.WebApi;

internal static class DependencyInjectionExtensions
{
    internal static void SetupOpenTelemetryLogging(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.ClearProviders()
            .AddConsole()
            .AddDebug()
            .AddOpenTelemetry(options =>
            {
                options
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(CommomConstants.ApplicationName))
                .IncludeScopes = true;
            });
    }

    internal static void AddOpenTelemetryServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = true)
    {
        string otelExportUrl = configuration["OtelExport:Endpoint"] ?? string.Empty;
        var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(CommomConstants.ApplicationName);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(CommomConstants.ApplicationName))
            .WithTracing(builder =>
            {
                builder
                .SetResourceBuilder(resourceBuilder)
                .AddSource(CommomConstants.ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => { options.Endpoint = new Uri(uriString: otelExportUrl); });

                if (isDevelopment)
                {
                    builder
                    .AddConsoleExporter()
                    .SetSampler<AlwaysOnSampler>();
                }
            })
            .WithMetrics(builder =>
            {
                builder
                .SetResourceBuilder(resourceBuilder)
                .AddMeter(CommomConstants.ApplicationName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddOtlpExporter(options => { options.Endpoint = new Uri(uriString: otelExportUrl); });

                if (isDevelopment)
                {
                    builder.AddConsoleExporter();
                }
            }).WithLogging(builder =>
            {
                if (isDevelopment)
                {
                    builder.AddConsoleExporter()
                    .AddOtlpExporter(options => { options.Endpoint = new Uri(uriString: otelExportUrl); });
                }
            });
    }
}
