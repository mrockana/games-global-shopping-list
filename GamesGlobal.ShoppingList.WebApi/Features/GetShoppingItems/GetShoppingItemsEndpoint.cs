using System.Collections.Generic;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.GetShoppingItems;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GamesGlobal.ShoppingList.WebApi.Features.GetShoppingItems;

internal sealed class GetShoppingItemsEndpoint : IEndpoint
{
    public void MapEndpoint(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapGet("/shopping-items",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite | Permissions.ShoppingItemsSelfReadOnly)]
        async ([FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            System.Security.Claims.ClaimsPrincipal? user = context.User;
            var request = new GetShoppingItemsQuery(user!.GetUserCode());
            var result = await requestProcessor.Process<GetShoppingItemsQuery, IList<GetShoppingItemResponse>>(request, context.RequestAborted);
            return result;
        })
       .WithName("GetShoppingItems")
       .Produces<IList<GetShoppingItemResponse>>()
       .AddEndpointFilter<ResponseHandlingFilter>()
       .WithTags(EndpointTags.ShoppingItem);
    }
}
