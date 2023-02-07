using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts.DocumentService;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class DocumentService : IDocumentService
    {
        private readonly IDbContextFactory<MainDbContext> dbFactory;
        public DocumentService(IDbContextFactory<MainDbContext> dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public string CreateUploadId()
        {
            return Guid.NewGuid().ToString();
        }

        public async Task<bool> SaveAsync(UploadedDocumentModel doc)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var entity = new UploadedDocument
            {
                Id = doc.UploadId,
                Name = doc.Name,
                Content = doc.Content
            };

            unitOfWork.UploadedDocumentRepository.Add(entity);
            var result = await unitOfWork.SaveAsync();
            return result == 1;
        }
    }
}
