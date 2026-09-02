namespace GamesGlobal.ShoppingList.Application.Common;

public static class Constants
{
    public const string ApplicationName = "GamesGlobal.ShoppingList";

    public static string AppVersion => typeof(Constants).Assembly.GetName().Version?.ToString() ?? "1.0";
}
