using System.IO;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImage;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;
using GamesGlobal.ShoppingList.BusinessDomain.Identity.Auth;
using GamesGlobal.ShoppingList.WebApi.Common.Endpoints;
using GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;
using GamesGlobal.ShoppingList.WebApi.Identity.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RateLimiterConstants = GamesGlobal.ShoppingList.WebApi.Common.RateLimiting.RateLimiterConstants;

namespace GamesGlobal.ShoppingList.WebApi.Features.UploadShoppingItemImage;

internal sealed class UploadShoppingItemImageEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        FileObjectStoreOptions fileObjectStoreOptions = app.ServiceProvider.GetRequiredService<IOptions<FileObjectStoreOptions>>().Value;

        app.MapPost("/shopping-items/{shoppingItemId:long}/upload-image",
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Permissions = Permissions.ShoppingItemsSelfReadWrite)]
        async ([FromRoute] long shoppingItemId, [FromForm] IFormFile file, [FromServices] ApplicationRequestProcessor requestProcessor, HttpContext context) =>
        {
            System.Security.Claims.ClaimsPrincipal? user = context.User;

            await using Stream content = file.OpenReadStream();

            // The Content-Disposition file name is caller supplied, so only the leaf name is ever used.
            var request = new UploadShoppingItemImageCommandRequest(
                UserCode: user!.GetUserCode(),
                ShoppingItemId: shoppingItemId,
                FileName: Path.GetFileName(file.FileName),
                Content: content,
                ContentType: file.ContentType,
                Length: file.Length);

            var result = await requestProcessor.Process<UploadShoppingItemImageCommandRequest, UploadShoppingItemImageResponse>(request, context.RequestAborted);

            return result;
        })
       .WithName("UploadShoppingItemImage")
       .Accepts<IFormFile>("multipart/form-data")
       .Produces<UploadShoppingItemImageResponse>()
       .AddEndpointFilter<ResponseHandlingFilter>()
       .DisableAntiforgery()
       .WithMetadata(new RequestSizeLimitAttribute(fileObjectStoreOptions.MaxImageSizeInBytes))
       .RequireRateLimiting(RateLimiterConstants.PerUserLimiterPolicyName)
       .WithTags(EndpointTags.ShoppingItem);
    }
}
