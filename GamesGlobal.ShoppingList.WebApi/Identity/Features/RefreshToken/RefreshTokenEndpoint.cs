using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.RefreshToken;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Identity.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.RefreshToken;

internal sealed class RefreshTokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/refresh-token",
    [Authorize(AuthenticationSchemes = "RefreshToken")]
        async ([FromBody] RefreshTokenRequest request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
    {
        System.Security.Claims.ClaimsPrincipal? user = context.User;
        var command = new RefreshTokenCommand(request.RefreshToken, user!.GetUserCode());
        var result = await requestProcessor.Process<RefreshTokenCommand, RefreshTokenResponse>(command, context.RequestAborted);
        return result;
    })
       .WithName("refresh-token")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<RefreshTokenResponse>()
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.Identity);
    }

    public sealed record RefreshTokenRequest(string RefreshToken);
}