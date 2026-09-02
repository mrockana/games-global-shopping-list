using System.Collections.Generic;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Identity.Features.GetRoles;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Identity.Features.GetRoles;

internal sealed class GetRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/identity/roles",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.UserRolesAndPermissionsReadOnly | Permissions.UserRolesAndPermissionsReadWrite)]
        async ([FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var request = new GetRolesQuery();
            var result = await requestProcessor.Process<GetRolesQuery, IList<GetRolesQueryResponse>>(request, context.RequestAborted);
            return result;
        })
       .WithName("get-roles")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<IList<GetRolesQueryResponse>>()
       .WithTags(EndpointTags.Identity);
    }
}
