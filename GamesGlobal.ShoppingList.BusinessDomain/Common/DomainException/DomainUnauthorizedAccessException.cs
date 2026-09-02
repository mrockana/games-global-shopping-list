using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainUnauthorizedAccessException : Exception
{
    public DomainUnauthorizedAccessException(string? message)
        : base(message)
    {
    }

    public DomainUnauthorizedAccessException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
