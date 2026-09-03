using System.IO;
using System.Threading;
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

    public async Task<bool> CreateBucket(string bucketName)
    {
        await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        return true;
    }

    public Task<bool> BucketExists(string bucketName, CancellationToken cancellationToken = default)
    {
        return _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
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

    public async Task<bool> UploadObject(string bucketName, string objectName, Stream data, string contentType, long size, CancellationToken cancellationToken = default)
    {
        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(data)
                .WithObjectSize(size)
                .WithContentType(contentType),
            cancellationToken);

        return true;
    }

    public async Task<bool> RemoveObject(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        await _minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName),
            cancellationToken);

        return true;
    }
}
