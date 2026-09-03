using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.DeleteShoppingItem;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Features.DeleteShoppingItem;

internal sealed class DeleteShoppingItemEndpoint : IEndpoint
{
    public void MapEndpoint(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapDelete("/shopping-item/{shoppingItemId}",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite)]
        async ([FromRoute] int shoppingItemId, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var request = new DeleteShoppingItemCommand(shoppingItemId, context.User.GetUserCode());

            var result = await requestProcessor.Process<DeleteShoppingItemCommand, DeleteShoppingItemResponse>(request, context.RequestAborted);
            return result;
        })
       .WithName("DeleteShoppingItem")
       .Produces<DeleteShoppingItemResponse>()
       .AddEndpointFilter<ResponseHandlingFilter>()
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.ShoppingItem);
    }
}
