using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;

public interface IFileObjectStoreService
{
    Task<bool> BucketExists(string bucketName, CancellationToken cancellationToken = default);

    Task<bool> CreateBucket(string bucketName);

    Task<bool> UploadObject(string bucketName, string objectName, Stream data);

    Task<bool> UploadObject(string bucketName, string objectName, Stream data, string contentType, long size, CancellationToken cancellationToken = default);

    Task<bool> RemoveObject(string bucketName, string objectName, CancellationToken cancellationToken = default);
}
