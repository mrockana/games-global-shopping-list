using System.Collections.Generic;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.SearchShoppingItems;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Features.SearchShoppingItems;

internal sealed class SearchShoppingItemsEndpoint : IEndpoint
{
    public void MapEndpoint(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder app)
    {
        app.MapGet("/shopping-items/search",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite | Permissions.ShoppingItemsSelfReadOnly)]
        async ([FromQuery] string search, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            System.Security.Claims.ClaimsPrincipal? user = context.User;
            var request = new SearchShoppingItemsQuery(user!.GetUserCode(), search);
            var result = await requestProcessor.Process<SearchShoppingItemsQuery, IList<SearchShoppingItemsResponse>>(request, context.RequestAborted);
            return result;
        })
       .WithName("SearchShoppingItems")
       .Produces<IList<SearchShoppingItemsResponse>>()
       .AddEndpointFilter<ResponseHandlingFilter>()
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.ShoppingItem);
    }
}
