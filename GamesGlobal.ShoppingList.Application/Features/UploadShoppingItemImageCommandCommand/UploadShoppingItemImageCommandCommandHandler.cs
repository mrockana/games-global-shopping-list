using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common;
using GamesGlobal.ShoppingList.Application.Common.RequestProcessor;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DataAccess.Repository;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using Microsoft.Extensions.Logging;

namespace GamesGlobal.ShoppingList.Application.Features.UploadShoppingItemImageCommandCommand;

public sealed class UploadShoppingItemImageCommandCommandHandler : IApplicationRequestHandler<UploadShoppingItemImageCommandCommandRequest, UploadShoppingItemImageCommandResponse>
{
    private readonly IApplicationRepository _repository;
    private readonly ILogger<UploadShoppingItemImageCommandCommandHandler> _logger;
    private readonly ActivitySource _activitySource;

    public UploadShoppingItemImageCommandCommandHandler(IApplicationRepository repository, ILogger<UploadShoppingItemImageCommandCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
        _activitySource = DiagnosticConfig.ActivitySource;
    }

    public async Task<Result<UploadShoppingItemImageCommandResponse>> Handle(UploadShoppingItemImageCommandCommandRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogError("Not Implemented");
        throw new DomainApplicationException("This is a upload shopping item image command handler. Please implement the handler logic here.");
    }
}

public sealed record UploadShoppingItemImageCommandCommandRequest(int UploadShoppingItemImageCommandId, string Name, string? Description)
    : ICommand<UploadShoppingItemImageCommandResponse>
{
}