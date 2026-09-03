using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.Login;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.RateLimiting;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.Login;

internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/login", async ([FromBody] SessionLoginCommand request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var result = await requestProcessor.Process<SessionLoginCommand, LoginResponse>(request, context.RequestAborted);
            return result;
        })
       .WithName("Login")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<LoginResponse>()
       .RequireRateLimiting(RateLimiterConstants.PerIpLimiterPolicyName)
       .WithTags(EndpointTags.Identity);
    }
}
