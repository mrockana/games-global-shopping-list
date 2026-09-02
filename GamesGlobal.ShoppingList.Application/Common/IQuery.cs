using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

namespace GamesGlobal.ShoppingList.Application.Common;

public interface IQuery<T> : IApplicationRequest<Result<T>>
{
}
