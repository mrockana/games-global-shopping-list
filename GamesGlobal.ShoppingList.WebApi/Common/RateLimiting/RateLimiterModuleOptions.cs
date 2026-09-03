namespace GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;

internal sealed class RateLimiterModuleOptions
{
    public required int WindowPermitLimit { get; set; }

    public required int TokenLimit { get; set; }

    public required int TokensPerPeriod { get; set; }

    public required int WindowTimeLimitInSeconds { get; set; }

    public required int ReplenishmentPeriodInSeconds { get; set; }
}
