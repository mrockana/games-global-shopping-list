using System.IO;
using System.Threading.Tasks;

namespace GamesGlobal.ShoppingList.BusinessDomain.Features.FileObjectStore;

public interface IFileObjectStoreService
{
    Task<bool> CreateBusket(string bucketName);

    Task<bool> UploadObject(string bucketName, string objectName, Stream data);
}
