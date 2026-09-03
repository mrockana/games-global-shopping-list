namespace GamesGlobal.ShoppingList.WebApi.Common;

internal static class WebApiConstants
{
    internal static class ProblemDetailTitleConstants
    {
        internal const string NotImplementedTitle = "Method not implemented.";
        internal const string DependencyTitle = "Dependency issue.";
        internal const string GeneralExceptionTitle = "Failed to process request.";
        internal const string TooManyRequestsTitle = "Too many requests.";
        internal const string NotFoundExceptionTitle = "Not found.";
        internal const string ValidationExceptionTitle = "Validation error.";
        internal const string ForbiddenActionExceptionTitle = "Forbidden action.";
        internal const string UnauthorizedExceptionTitle = "Unauthorized action.";
    }
}
