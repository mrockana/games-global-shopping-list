using FluentValidation.Results;
using GamesGlobal.ShoppingList.Application.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace GamesGlobal.ShoppingList.xUnitTests.Helpers;

internal static class AssertionHelper
{
    internal static void AssertLoggerShouldLogMessage<T>(ILogger<T> logger, string message, int requiredNumberOfCalls = 1, LogLevel logLevel = LogLevel.Information)
        where T : class
    {
        logger.Received(requiredNumberOfCalls: requiredNumberOfCalls).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o =>
                    o.ToString()
                    !.Contains(message)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    internal static void AssertLoggerShouldNotLogMessage<T>(ILogger<T> logger, string message, LogLevel logLevel = LogLevel.Information)
    where T : class
    {
        logger.DidNotReceive().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o =>
                    o.ToString()
                    !.Contains(message)),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    internal static void AssertValidationShouldHaveError(this ValidationResult result, string propertyName)
    {
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
        e.PropertyName.Equals(propertyName, StringComparison.InvariantCulture));
    }

    internal static void AssertValidationShouldHaveError(this ValidationResult result, string propertyName, string validationMessage)
    {
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
        e.PropertyName.Equals(propertyName, StringComparison.InvariantCulture) &&
        e.ErrorMessage.Contains(validationMessage, StringComparison.InvariantCulture));
    }

    internal static void AssertIsErrorResult<T>(this Result<T> result, Type exceptionType, string errorMessage)
        where T : class
    {
        Assert.True(result.HasError);
        Assert.IsType(exceptionType, result.Error);
        Assert.Null(result.Value);
        Assert.Contains(errorMessage, result.Error.Message, StringComparison.InvariantCulture);
    }

    internal static void AssertIsErrorResult<T>(this Result<T> result, Type exceptionType)
    where T : class
    {
        Assert.True(result.HasError);
        Assert.IsType(exceptionType, result.Error);
        Assert.Null(result.Value);
    }
}
