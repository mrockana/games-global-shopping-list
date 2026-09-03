using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.UpdateShoppingItemCommand;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Features.UpdateShoppingItem;

internal sealed class UpdateShoppingItemEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/update-shopping-item",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite)]
        async ([FromBody] UpdateShoppingItemCommandRequest shoppingItem, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var result = await requestProcessor.Process<UpdateShoppingItemCommandRequest, UpdateShoppingItemResponse>(shoppingItem, context.RequestAborted);
            return result;
        })
       .WithName("UpdateShoppingItem")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<UpdateShoppingItemResponse>()
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.ShoppingItem);
    }
}
