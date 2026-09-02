using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainDependencyException : Exception
{
    public DomainDependencyException(string? message)
    : base(message)
    {
    }

    public DomainDependencyException(string? message, Exception? innerException)
    : base(message, innerException)
    {
    }
}
