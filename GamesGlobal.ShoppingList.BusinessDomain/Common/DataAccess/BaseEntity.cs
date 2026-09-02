using System;

namespace GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess;

public abstract class BaseEntity
{
    public DateTime Created { get; set; }

    public DateTime? Modified { get; set; }
}
