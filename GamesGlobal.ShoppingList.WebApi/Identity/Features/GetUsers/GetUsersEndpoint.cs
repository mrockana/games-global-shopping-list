using System.Collections.Generic;
using GamesGlobal.ShoppingList.Application.Common.Pagination;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetUsers;
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

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.GetUsers;

internal sealed class GetUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/identity/users",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadOnly | Permissions.UserRolesAndPermissionsReadWrite)]
        async ([FromQuery] string take, [FromQuery] string skip, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var takeParsed = int.TryParse(take, out int takeNumber);
            var skipParsed = int.TryParse(skip, out int skipNumber);
            var request = new GetUsersQuery(
                    Take: takeParsed ? takeNumber : 10,
                    Skip: skipParsed ? skipNumber : 0);
            var result = await requestProcessor.Process<GetUsersQuery, PaginatedResults<IList<GetUsersQueryResponse>>>(request, context.RequestAborted);
            return result;
        })
       .WithName("get-users")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<PaginatedResults<IList<GetUsersQueryResponse>>>()
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.Identity);
    }
}
