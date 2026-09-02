namespace GamesGlobal.ShoppingList.WebApi.Common;

internal static class WebApiConstants
{
    internal static class ProblemDetailTitleConstants
    {
        internal const string NotImplementedTitle = "Method Not Implemented.";
        internal const string DependencyTitle = "Dependency Issue.";
        internal const string GeneralExceptionTitle = "Failed to Process Request.";
        internal const string NotFoundExceptionTitle = "Not Found.";
        internal const string ValidationExceptionTitle = "Validation Error.";
        internal const string ForbiddenActionExceptionTitle = "Forbidden Action.";
        internal const string UnauthorizedExceptionTitle = "Unauthorized Action.";
    }
}
