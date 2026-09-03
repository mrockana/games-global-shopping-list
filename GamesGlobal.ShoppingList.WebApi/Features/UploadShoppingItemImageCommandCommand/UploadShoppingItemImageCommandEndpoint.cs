using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImageCommandCommand;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GamesGlobal.ShoppingList.WebApi.Features.UploadShoppingItemImageCommand;

internal sealed class UploadShoppingItemImageCommandEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/shopping-items/{shoppingItemId}/image",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.None)]
        async ([FromRoute] long shoppingItemId, [FromBody] UploadShoppingItemImageCommandCommandRequest featureNameRequest, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            var result = await requestProcessor.Process<UploadShoppingItemImageCommandCommandRequest, UploadShoppingItemImageCommandResponse>(featureNameRequest, context.RequestAborted);
            return result;
        })
       .WithName("upload-shopping-item-image-command")
       .AddEndpointFilter<ResponseHandlingFilter>()
       .Produces<UploadShoppingItemImageCommandResponse>()
       .WithTags(EndpointTags.Default);
    }
}
