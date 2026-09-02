namespace GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

public interface IApplicationRequest<out T>
   where T : BaseResult
{
}
