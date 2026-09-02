using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Common.Endpoints;

internal interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
