using System;
using System.Collections.Generic;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;

public class DomainValidationException : Exception
{
    public DomainValidationException(string? message)
        : base(message)
    {
    }

    public DomainValidationException(string? message, IList<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public DomainValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public IList<string> Errors { get; init; } = Array.Empty<string>();
}
