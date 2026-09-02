using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

namespace GamesGlobal.ShoppingList.Application.Common;

public interface ICommand<T> : IApplicationRequest<Result<T>>
{
}
