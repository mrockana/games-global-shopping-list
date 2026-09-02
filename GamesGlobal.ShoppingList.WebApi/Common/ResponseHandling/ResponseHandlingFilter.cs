using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using GamesGlobal.ShoppingList.BusinessDomain.Common.EnumHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static GamesGlobal.ShoppingList.WebApi.Common.WebApiConstants;

namespace GamesGlobal.ShoppingList.WebApi.Common.ResponseHandling;

internal sealed class ResponseHandlingFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? rawResult = await next(context);

        var result = CreateResult(rawResult!);

        if (!result!.HasError)
        {
            if (result.Value != null)
            {
                return Results.Ok(result.Value);
            }

            return Results.NoContent();
        }

        Activity.Current!.AddException(result.Error!);
        ProblemDetails problemDetails = new();

        if (result.Error is NotImplementedException notImplementedEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.NotImplementedTitle;
            problemDetails.Type = HttpStatusCode.NotImplemented.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.NotImplemented;
            problemDetails.Detail = notImplementedEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainDependencyException dependencyEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.DependencyTitle;
            problemDetails.Type = HttpStatusCode.InternalServerError.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
            problemDetails.Detail = dependencyEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainApplicationException applicationEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.GeneralExceptionTitle;
            problemDetails.Type = HttpStatusCode.InternalServerError.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
            problemDetails.Detail = applicationEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainNotFoundException notFoundEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.NotFoundExceptionTitle;
            problemDetails.Type = HttpStatusCode.NotFound.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.NotFound;
            problemDetails.Detail = notFoundEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainValidationException validationEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.ValidationExceptionTitle;
            problemDetails.Type = HttpStatusCode.BadRequest.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.BadRequest;
            problemDetails.Detail = validationEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainForbiddenActionException forbiddenEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.ForbiddenActionExceptionTitle;
            problemDetails.Type = HttpStatusCode.Forbidden.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.Forbidden;
            problemDetails.Detail = forbiddenEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is DomainUnauthorizedAccessException domainUnauthorizedEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.UnauthorizedExceptionTitle;
            problemDetails.Type = HttpStatusCode.Unauthorized.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.Unauthorized;
            problemDetails.Detail = domainUnauthorizedEx.Message;
            return Results.Problem(problemDetails);
        }

        if (result.Error is UnauthorizedAccessException unauthorizedEx)
        {
            problemDetails.Title = ProblemDetailTitleConstants.UnauthorizedExceptionTitle;
            problemDetails.Type = HttpStatusCode.Unauthorized.GetDescription();
            problemDetails.Status = (int)HttpStatusCode.Unauthorized;
            problemDetails.Detail = unauthorizedEx.Message;
            return Results.Problem(problemDetails);
        }

        problemDetails.Title = ProblemDetailTitleConstants.GeneralExceptionTitle;
        problemDetails.Type = HttpStatusCode.InternalServerError.GetDescription();
        problemDetails.Status = (int)HttpStatusCode.InternalServerError;
        problemDetails.Detail = "Error";
        return Results.Problem(problemDetails);
    }

    private static Result<object> CreateResult(object obj)
    {
        dynamic dynamicObj = obj;
        var resultGenericType = typeof(Result<object>);

        if (dynamicObj.HasError)
        {
            var newErrorResultInstance = Activator.CreateInstance(resultGenericType, dynamicObj.Error);
            return (Result<object>)newErrorResultInstance!;
        }

        var newResultInstance = Activator.CreateInstance(resultGenericType, dynamicObj.Value);
        return (Result<object>)newResultInstance!;
    }
}
