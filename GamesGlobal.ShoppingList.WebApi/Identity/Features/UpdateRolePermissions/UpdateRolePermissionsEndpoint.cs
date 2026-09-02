using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.UpdateRolePermissions;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.UpdateRolePermissions;

internal sealed class UpdateRolePermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/update-role-permissions",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadWrite)]
        async ([FromBody] UpdateRolePermissionsCommand request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
    {
        var result = await requestProcessor.Process<UpdateRolePermissionsCommand, UpdateRolePermissionsResponse>(request, context.RequestAborted);
        return result;
    })
       .WithName("update-role-permissions")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<UpdateRolePermissionsResponse>()
       .WithTags(EndpointTags.Identity);
    }
}
