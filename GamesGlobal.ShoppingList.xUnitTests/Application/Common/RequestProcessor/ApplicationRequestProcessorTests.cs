using System.Net.Http;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace GamesGlobal.ShoppingList.xUnitTests.Application.Common.RequestProcessor;

public sealed class ApplicationRequestProcessorTests
{
    [Fact]
    public async Task Process_NoValidators_HandlerSucceeds_ReturnsSuccessResult()
    {
        var expectedResponse = new ProcessorTestResponse { Value = "success" };
        var (processor, _) = ApplicationRequestProcessorTestsHelper.BuildProcessor(handlerResult: new Result<ProcessorTestResponse>(expectedResponse));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.False(result.HasError);
        Assert.Equal("success", result.Value!.Value);
    }

    [Fact]
    public async Task Process_ValidatorPasses_HandlerSucceeds_ReturnsSuccessResult()
    {
        var expectedResponse = new ProcessorTestResponse { Value = "valid" };
        var (processor, _) = ApplicationRequestProcessorTestsHelper.BuildProcessor(
            validators: [new PassingTestValidator()],
            handlerResult: new Result<ProcessorTestResponse>(expectedResponse));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.False(result.HasError);
        Assert.Equal("valid", result.Value!.Value);
    }

    [Fact]
    public async Task Process_OneValidatorFails_ReturnsValidationError_HandlerNotCalled()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper
            .BuildProcessor(validators: [new FailingTestValidator("Name is required")]);

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainValidationException>(result.Error);
        await handler.DidNotReceive().Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_MultipleValidators_OneFails_ReturnsValidationError()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper
            .BuildProcessor(validators: [new PassingTestValidator(), new FailingTestValidator("Email is required")]);

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainValidationException>(result.Error);
        Assert.Contains("Email is required", result.Error.Message, StringComparison.Ordinal);
        await handler.DidNotReceive().Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_MultipleValidators_AllPass_HandlerCalledOnce()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor(
            validators: [new PassingTestValidator(), new PassingTestValidator()]);

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.False(result.HasError);
        await handler.Received(1).Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Process_HandlerThrowsHttpRequestException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new HttpRequestException("Service unavailable"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<HttpRequestException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainDependencyException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainDependencyException("Dependency failed"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainDependencyException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainApplicationException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();

        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainApplicationException("App error"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainApplicationException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainValidationException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainValidationException("Invalid data"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainValidationException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainNotFoundException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainNotFoundException("Not found"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainNotFoundException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainForbiddenActionException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainForbiddenActionException("Forbidden"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainForbiddenActionException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsUnauthorizedAccessException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new UnauthorizedAccessException("Unauthorized"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<UnauthorizedAccessException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsDomainUnauthorizedAccessException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new DomainUnauthorizedAccessException("Domain unauthorized"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<DomainUnauthorizedAccessException>(result.Error);
    }

    [Fact]
    public async Task Process_HandlerThrowsGenericException_ReturnsErrorResult()
    {
        var (processor, handler) = ApplicationRequestProcessorTestsHelper.BuildProcessor();
        handler.Handle(Arg.Any<ProcessorTestRequest>(), Arg.Any<CancellationToken>())
               .Throws(new Exception("Unexpected error"));

        var result = await processor.Process<ProcessorTestRequest, ProcessorTestResponse>(new ProcessorTestRequest());

        Assert.True(result.HasError);
        Assert.IsType<Exception>(result.Error);
        Assert.Equal("Unexpected error", result.Error.Message);
    }
}
