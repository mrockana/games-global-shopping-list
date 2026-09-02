using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.AddRole;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.AddRole;

internal sealed class AddRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/identity/add-role",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadWrite)]
        async ([FromBody] AddRoleCommand request, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
            {
                var result = await requestProcessor.Process<AddRoleCommand, AddRoleResponse>(request, context.RequestAborted);
                return result;
            })
       .WithName("add-role")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<AddRoleResponse>()
       .WithTags(EndpointTags.Identity);
    }
}
