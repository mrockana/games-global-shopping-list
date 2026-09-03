using System.Collections.Generic;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;

public sealed class FileObjectStoreOptions
{
    public string Url { get; set; } = string.Empty;

    public string User { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public bool UseSsl { get; set; }

    public string BucketName { get; set; } = string.Empty;

    public long MaxImageSizeInBytes { get; set; } = 10 * 1024 * 1024;

    public int MaxObjectNameLength { get; set; } = 200;

    public IReadOnlyCollection<string> AllowedContentTypes { get; set; } =
    [
        "image/png",
        "image/jpeg"
    ];
}
