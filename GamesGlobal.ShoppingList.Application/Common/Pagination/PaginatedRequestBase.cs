namespace GamesGlobal.ShoppingList.Application.Common.Pagination;

public abstract record PaginatedRequestBase(int Take, int Skip);
