using System.Diagnostics;
using System.Diagnostics.Metrics;
using GamesGlobal.ShoppingList.Application.Common;

namespace GamesGlobal.ShoppingList.Application;

public static class DiagnosticConfig
{
    public const string SourceName = "GamesGlobal.ShoppingList.Application";

    public static readonly Meter AppMeter = new(Constants.ApplicationName, Constants.AppVersion);

    public static ActivitySource ActivitySource => new(Constants.ApplicationName, Constants.AppVersion);

    public static Counter<long> UpdateShoppingItemCounter => AppMeter.CreateCounter<long>("update.shopping.item", description: "Counts number of times a user updates an item");

    public static Counter<long> SearchShoppingItemsCounter => AppMeter.CreateCounter<long>("search.shopping.items", description: "Counts shopping-item searches per user");

    public static Histogram<double> SearchShoppingItemsDuration => AppMeter.CreateHistogram<double>("search.shopping.items.duration", "s", "Duration of shopping items searches");
}
