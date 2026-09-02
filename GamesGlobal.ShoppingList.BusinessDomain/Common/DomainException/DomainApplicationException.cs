using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainApplicationException : Exception
{
    public DomainApplicationException(string? message)
        : base(message)
    {
    }

    public DomainApplicationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
