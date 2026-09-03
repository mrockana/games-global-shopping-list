using System;
using System.Threading.RateLimiting;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;

internal static class DependencyInjectionExtensionsRateLimiting
{
    internal static void SetupRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection rateLimiterOptionsSection = configuration.GetRequiredSection(nameof(RateLimiterModuleOptions));
        services.Configure<RateLimiterModuleOptions>(rateLimiterOptionsSection);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimiterConstants.PerIpLimiterPolicyName, httpContext =>
                GetIpPartition(httpContext));

            options.AddPolicy(RateLimiterConstants.PerUserLimiterPolicyName, httpContext =>
            {
                var userCode = httpContext.User.GetUserCode();

                if (userCode != Guid.Empty)
                {
                    var rateLimiterOptions = GetRateLimiterOptions(httpContext);

                    return RateLimitPartition.GetTokenBucketLimiter(userCode.ToString(), limiterOptions => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = rateLimiterOptions.TokenLimit,
                        TokensPerPeriod = rateLimiterOptions.TokensPerPeriod,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimiterOptions.ReplenishmentPeriodInSeconds),
                    });
                }

                return GetIpPartition(httpContext);
            });
        });
    }

    private static RateLimitPartition<string> GetIpPartition(HttpContext httpContext)
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? RateLimiterConstants.UnknownIpPartitionKey;
        var rateLimiterOptions = GetRateLimiterOptions(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, limiterOptions =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimiterOptions.WindowPermitLimit,
                Window = TimeSpan.FromSeconds(rateLimiterOptions.WindowTimeLimitInSeconds),
            });
    }

    private static RateLimiterModuleOptions GetRateLimiterOptions(HttpContext httpContext) =>
        httpContext.RequestServices.GetRequiredService<IOptions<RateLimiterModuleOptions>>().Value;
}
