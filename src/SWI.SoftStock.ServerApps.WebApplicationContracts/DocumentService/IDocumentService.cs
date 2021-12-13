using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.DocumentService
{
    public interface IDocumentService
    {
        string CreateUploadId();
        Task<bool> SaveAsync(UploadedDocumentModel doc);
    }
}
