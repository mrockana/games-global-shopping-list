using System;

namespace GamesGlobal.ShoppingList.Application.Common;

public sealed class Result<T> : BaseResult
{
    public Result()
    {
    }

    public Result(T result)
    {
        Value = result;
    }

    public Result(Exception ex)
    {
        Error = ex;
    }

    public T? Value { get; set; }

    public static explicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static explicit operator Result<T>(Exception ex)
    {
        return new Result<T>(ex);
    }
}

internal static class Result
{
    public static object CreateErrorResult<T>(this Type responseType, Exception ex)
        where T : BaseResult
    {
        var errorResultType = typeof(Result<T>)
            .GetGenericTypeDefinition()
            .MakeGenericType(responseType.GenericTypeArguments[0]);

        var newErrorResultInstance = Activator.CreateInstance(errorResultType, ex);
        return newErrorResultInstance!;
    }

    public static Result<T> CreateErrorResult<T>(Exception ex)
    {
        return new Result<T>(ex);
    }

    public static Result<T> CreateResult<T>(T value)
    {
        return new Result<T>(value);
    }
}
