using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Common.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
