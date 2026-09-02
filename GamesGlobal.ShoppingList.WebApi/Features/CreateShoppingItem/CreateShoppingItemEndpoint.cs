using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.CreateShoppingItem;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GamesGlobal.ShoppingList.WebApi.Features.CreateShoppingItem;

internal sealed class CreateShoppingItemEndpoint : IEndpoint
{
    public void MapEndpoint(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapPost("/create-shopping-item",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite)]
        async ([FromBody] CreateShoppingItemEndpointRequest shoppingItem, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            System.Security.Claims.ClaimsPrincipal? user = context.User;
            var request = new CreateShoppingItemCommandRequest(UserCode: user!.GetUserCode(), shoppingItem.Name, shoppingItem.Description);

            var result = await requestProcessor.Process<CreateShoppingItemCommandRequest, CreateShoppingItemResponse>(request, context.RequestAborted);

            return result;
        })
       .WithName("CreateShoppingItem")
       .Produces<CreateShoppingItemResponse>()
       .AddEndpointFilter<ResponseHandlingFilter>()
       .WithTags(EndpointTags.ShoppingItem);
    }
}

internal sealed record CreateShoppingItemEndpointRequest(string Name, string Description);
