using System;
using FluentValidation;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Common.RequestProcessor;

public static class ApplicationRequestProcessorTestsHelper
{
    public static (ApplicationRequestProcessor processor,
                    IApplicationRequestHandler<ProcessorTestRequest, ProcessorTestResponse> handler)
        BuildProcessor(
            IValidator<ProcessorTestRequest>[]? validators = null,
            Result<ProcessorTestResponse>? handlerResult = null)
    {
        var handler = Substitute.For<IApplicationRequestHandler<ProcessorTestRequest, ProcessorTestResponse>>();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Returns(handlerResult ?? new Result<ProcessorTestResponse>(new ProcessorTestResponse { Value = "ok" }));

        var services = new ServiceCollection();
        services.AddSingleton(handler);

        if (validators is { Length: > 0 })
        {
            foreach (var v in validators)
            {
                services.AddSingleton<IValidator<ProcessorTestRequest>>(v);
            }
        }

        services.AddLogging();

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ILogger<ApplicationRequestProcessor>>();

        var processor = new ApplicationRequestProcessor(provider, logger);
        return (processor, handler);
    }
}

public sealed class ProcessorTestRequest : IApplicationRequest<Result<ProcessorTestResponse>>
{
}

public sealed class ProcessorTestResponse
{
    public string Value { get; set; } = string.Empty;
}

public sealed class PassingTestValidator : AbstractValidator<ProcessorTestRequest>
{
}

public sealed class FailingTestValidator : AbstractValidator<ProcessorTestRequest>
{
    public FailingTestValidator(string errorMessage = "Validation failed")
    {
        RuleFor(x => x).Must(_ => false).WithMessage(errorMessage);
    }
}
