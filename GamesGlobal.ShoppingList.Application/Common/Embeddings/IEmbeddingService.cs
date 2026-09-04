using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Pgvector;

namespace GamesGlobal.ShoppingList.Application.Common.Embeddings;

public interface IEmbeddingService
{
    Task<IReadOnlyList<Vector>> GenerateAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken = default);
}
