using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.SignupUser;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.SignupUser;

internal sealed class UserSignupEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/signup", async ([FromBody] SignupCommand request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var result = await requestProcessor.Process<SignupCommand, SignupResponse>(request, context.RequestAborted);

            return result;
        })
       .WithName("RegisterUser")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<SignupResponse>()
       .RequireRateLimiting(RateLimiterConstants.PerIpLimiterPolicyName)
       .WithTags(EndpointTags.Identity);
    }
}
