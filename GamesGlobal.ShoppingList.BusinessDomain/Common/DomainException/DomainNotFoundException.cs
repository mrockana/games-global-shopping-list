using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainNotFoundException : Exception
{
    public DomainNotFoundException(string? message)
    : base(message)
    {
    }

    public DomainNotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
