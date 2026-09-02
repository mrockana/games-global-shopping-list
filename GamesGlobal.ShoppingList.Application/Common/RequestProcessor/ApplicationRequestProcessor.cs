using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

public sealed class ApplicationRequestProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApplicationRequestProcessor> _logger;
    public ApplicationRequestProcessor(IServiceProvider serviceProvider, ILogger<ApplicationRequestProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<Result<TResponse>> Process<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IApplicationRequest<Result<TResponse>>
        where TResponse : class
    {
        try
        {
            var applicationRequestHandler = GetRequestHandlerService<TRequest, TResponse>();

            var (isValid, result) = ValidateRequest<TRequest, TResponse>(request);

            if (!isValid)
            {
                return result!;
            }

            return await applicationRequestHandler.Handle(request, cancellationToken);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "{Caller} - {Message}", nameof(Handle), httpEx.Message);

            var result = Result.CreateErrorResult<TResponse>(httpEx);

            return result;
        }
        catch (DomainDependencyException domainEx)
        {
            _logger.LogError(domainEx, "{Caller} - {Message}", nameof(Handle), domainEx.Message);
            var result = Result.CreateErrorResult<TResponse>(domainEx);
            return result;
        }
        catch (DomainApplicationException applicationEx)
        {
            _logger.LogError(applicationEx, "{Caller} - {Message}", nameof(Handle), applicationEx.Message);
            var result = Result.CreateErrorResult<TResponse>(applicationEx);
            return result;
        }
        catch (DomainValidationException validationEx)
        {
            _logger.LogError(validationEx, "{Caller} - {Message}", nameof(Handle), validationEx.Message);
            var result = Result.CreateErrorResult<TResponse>(validationEx);
            return result;
        }
        catch (DomainNotFoundException notFoundEx)
        {
            _logger.LogError(notFoundEx, "{Caller} - {Message}", nameof(Handle), notFoundEx.Message);
            var result = Result.CreateErrorResult<TResponse>(notFoundEx);
            return result;
        }
        catch (DomainForbiddenActionException forbiddenEx)
        {
            _logger.LogError(forbiddenEx, "{Caller} - {Message}", nameof(Handle), forbiddenEx.Message);
            var result = Result.CreateErrorResult<TResponse>(forbiddenEx);
            return result;
        }
        catch (UnauthorizedAccessException unauthorizedEx)
        {
            _logger.LogError(unauthorizedEx, "{Caller} - {Message}", nameof(Handle), unauthorizedEx.Message);
            var result = Result.CreateErrorResult<TResponse>(unauthorizedEx);
            return result;
        }
        catch (DomainUnauthorizedAccessException unauthorizedEx)
        {
            _logger.LogError(unauthorizedEx, "{Caller} - {Message}", nameof(Handle), unauthorizedEx.Message);
            var result = Result.CreateErrorResult<TResponse>(unauthorizedEx);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Caller} - {Message}", nameof(Handle), ex.Message);
            var result = Result.CreateErrorResult<TResponse>(ex);
            return result;
        }
    }

    private (bool isValid, Result<TResponse>? result) ValidateRequest<TRequest, TResponse>(TRequest request)
        where TRequest : class
        where TResponse : class
    {
        var validators = GetValidators<TRequest>();

        if (!validators.Any())
        {
            return (true, null);
        }

        var context = new ValidationContext<TRequest>(request);

        var errors = validators
            .Select(x => x.Validate(context))
            .SelectMany(x => x.Errors)
            .Where(x => x != null).ToList();

        if (errors.Count > 0)
        {
            var exception = new DomainValidationException(JsonSerializer.Serialize(errors.Select(er => er.ErrorMessage)));
            var validationResult = typeof(Result<TResponse>).CreateErrorResult<Result<TResponse>>(exception);

            return (false, (Result<TResponse>)validationResult);
        }

        return (true, null);
    }

    private IEnumerable<IValidator<TRequest>> GetValidators<TRequest>()
        where TRequest : class
    {
        var validators = _serviceProvider.GetService<IEnumerable<IValidator<TRequest>>>();
        return validators ?? Array.Empty<IValidator<TRequest>>();
    }

    private IApplicationRequestHandler<TRequest, TResponse> GetRequestHandlerService<TRequest, TResponse>()
        where TRequest : class, IApplicationRequest<Result<TResponse>>
        where TResponse : class
    {
        var handlerType = typeof(IApplicationRequestHandler<,>).MakeGenericType(typeof(TRequest), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        // Cast to the correct interface so you can call Execute
        var applicationRequest = (IApplicationRequestHandler<TRequest, TResponse>)handler;

        return applicationRequest;
    }
}
