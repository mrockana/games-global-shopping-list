namespace GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;

public sealed class FileObjectStoreOptions
{
    public string Url { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    public string BusketName { get; set; } = string.Empty;
}
