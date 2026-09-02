using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainForbiddenActionException : Exception
{
    public DomainForbiddenActionException(string? message)
    : base(message)
    {
    }

    public DomainForbiddenActionException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
