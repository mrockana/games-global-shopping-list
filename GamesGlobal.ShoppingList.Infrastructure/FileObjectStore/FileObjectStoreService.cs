using System.IO;
using System.Threading.Tasks;
using GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;
using Minio;
using Minio.DataModel.Args;

namespace GamesGlobal.ShoppingList.Infrastructure.FileObjectStore;

public sealed class FileObjectStoreService : IFileObjectStoreService
{
    private readonly IMinioClient _minioClient;

    public FileObjectStoreService(IMinioClient minioClient)
    {
        _minioClient = minioClient;
    }

    public async Task<bool> CreateBusket(string bucketName)
    {
        await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        return true;
    }

    public async Task<bool> UploadObject(string bucketName, string objectName, Stream data)
    {
        await _minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length));

        return true;
    }
}
