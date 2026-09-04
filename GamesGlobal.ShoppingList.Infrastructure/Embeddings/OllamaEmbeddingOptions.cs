namespace GamesGlobal.ShoppingList.Infrastructure.Embeddings;

public sealed class OllamaEmbeddingOptions
{
    public string Url { get; init; } = string.Empty;

    public string Model { get; init; } = string.Empty;

    public int Dimensions { get; init; }

    public string EmbedEndpoint { get; init; } = string.Empty;
}