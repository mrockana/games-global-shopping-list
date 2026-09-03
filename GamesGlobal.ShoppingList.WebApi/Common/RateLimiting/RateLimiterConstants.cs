namespace GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;

internal static class RateLimiterConstants
{
    internal const string PerIpLimiterPolicyName = "per-ip";

    internal const string PerUserLimiterPolicyName = "per-user";

    internal const string UnknownIpPartitionKey = "unknown-ip";
}
