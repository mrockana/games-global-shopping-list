using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateUserRoles;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.UpdateUserRoles;

internal sealed class UpdateUserRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/update-user-roles",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadWrite)]
        async ([FromBody] UpdateUserRolesCommand request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
    {
        var result = await requestProcessor.Process<UpdateUserRolesCommand, UpdateUserRolesResponse>(request, context.RequestAborted);
        return result;
    })
       .WithName("update-user-roles")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<UpdateUserRolesResponse>()
       .WithTags(EndpointTags.Identity);
    }
}
