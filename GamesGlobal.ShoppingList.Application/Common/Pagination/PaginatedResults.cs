namespace GamesGlobal.ShoppingList.Application.Common.Pagination;

public sealed record PaginatedResults<T>(T Data, int TotalRecords, int PageSize, int TotalPages, int CurrentPage);
