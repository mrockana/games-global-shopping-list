using System.Collections.Generic;

namespace GamesGlobal.ShoppingList.Application.Common.Pagination;

public sealed record PaginatedResults<T>(IList<T> Data, int TotalRecords, int PageSize, int TotalPages, int CurrentPage);
