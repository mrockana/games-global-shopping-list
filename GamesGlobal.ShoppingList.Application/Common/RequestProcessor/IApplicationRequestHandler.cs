using System.Threading;
using System.Threading.Tasks;

namespace GamesGlobal.ShoppingList.Application.Common.RequestProcessor;

public interface IApplicationRequestHandler<in TRequest, TResponse>
    where TResponse : class
    where TRequest : class, IApplicationRequest<Result<TResponse>>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken = default);
}
