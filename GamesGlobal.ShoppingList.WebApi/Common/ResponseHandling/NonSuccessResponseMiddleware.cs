using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GamesGlobal.ShoppingList.WebApi.Common.WebApiConstants;

namespace GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;

internal sealed class NonSuccessResponseMiddleware : IMiddleware
{
    private readonly ILogger<NonSuccessResponseMiddleware> _logger;
    public NonSuccessResponseMiddleware(ILogger<NonSuccessResponseMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await next(context);

            if (context.Response.StatusCode >= 400 && memoryStream.Length == 0)
            {
                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                HttpStatusCode statusCode = (HttpStatusCode)context.Response.StatusCode;

                var title = context.Response.StatusCode == StatusCodes.Status429TooManyRequests
                    ? ProblemDetailTitleConstants.TooManyRequestsTitle
                    : ProblemDetailTitleConstants.GeneralExceptionTitle;

                ProblemDetails problemDetails = new();
                problemDetails.Title = title;
                problemDetails.Type = statusCode.GetDescription();
                problemDetails.Status = context.Response.StatusCode;
                problemDetails.Detail = title;
                problemDetails.Extensions.Add("traceId", traceId);

                await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
            }
        }
        catch (TaskCanceledException)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogWarning("Request was cancelled: {TrackingNumber}", traceId);
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            memoryStream.Seek(0, SeekOrigin.Begin);
        }
        catch (OperationCanceledException)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogWarning("Request was cancelled: {TrackingNumber}", traceId);
            context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            memoryStream.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            _logger.LogError(ex, "An error occurred while processing the request in the middleware: {TrackingNumber}", traceId);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            ProblemDetails problemDetails = new();
            problemDetails.Title = ProblemDetailTitleConstants.GeneralExceptionTitle;
            problemDetails.Type = HttpStatusCode.InternalServerError.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
            problemDetails.Detail = ex.Message;
            problemDetails.Extensions.Add("traceId", traceId);

            await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
        }
        finally
        {
            // Reset the response body stream position to the beginning
            memoryStream.Seek(0, SeekOrigin.Begin);

            if (!context.RequestAborted.IsCancellationRequested)
            {
                // Copy the contents of the memory stream to the original response body stream
                await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);

                // Restore the original response body stream
                context.Response.Body = originalBodyStream;
            }
        }
    }
}