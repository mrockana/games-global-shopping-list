using System;

namespace GamesGlobal.ShoppingList.Application.Common;

public class BaseResult
{
    public bool HasError => Error != null;

    public Exception? Error { get; set; }
}
