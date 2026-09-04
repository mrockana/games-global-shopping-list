using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.Application.Common.Embeddings;
using GamesGlobal.ShoppingList.BusinessDomain.Common.DomainException;
using Pgvector;

namespace GamesGlobal.ShoppingList.Infrastructure.Embeddings;

internal sealed class OllamaEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaEmbeddingOptions _options;

    public OllamaEmbeddingService(HttpClient httpClient, OllamaEmbeddingOptions options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<IReadOnlyList<Vector>> GenerateAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
    {
        HttpResponseMessage httpResponse = await _httpClient.PostAsJsonAsync(
            _options.EmbedEndpoint,
            new OllamaEmbedRequest(_options.Model, inputs),
            cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        OllamaEmbedResponseBody? content = await httpResponse.Content.ReadFromJsonAsync<OllamaEmbedResponseBody>(cancellationToken);
        if (content?.Embeddings is null || content.Embeddings.Length != inputs.Count)
        {
            throw new DomainDependencyException("Ollama did not return an embedding for every input.");
        }

        return content.Embeddings.Select(embedding =>
        {
            if (embedding.Length != _options.Dimensions)
            {
                throw new DomainDependencyException($"Ollama returned an embedding with {embedding.Length} dimensions; expected {_options.Dimensions.ToString()}.");
            }

            return new Vector(embedding);
        }).ToList();
    }

    private sealed record OllamaEmbedRequest(string Model, IReadOnlyList<string> Input);

    private sealed record OllamaEmbedResponseBody(float[][]? Embeddings);
}