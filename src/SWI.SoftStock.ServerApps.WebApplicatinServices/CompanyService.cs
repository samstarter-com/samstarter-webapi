using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.CompanyService.Add;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{

    public class CompanyService : ICompanyService
    {
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public CompanyService(IDbContextFactory<MainDbContext> dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<CompanyAddResponse> Add(StructureUnitModel model)
        {
            var company = new StructureUnit
            {
                Name = model.Name,
                ShortName = model.Name,
                UnitType = UnitType.Company
            };

            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            if (!IsCompanyExists(unitOfWork, company.Name))
            {
                unitOfWork.StructureUnitRepository.Add(company);
                await unitOfWork.SaveAsync();
                return new CompanyAddResponse() { CompanyUniqueId = company.UniqueId, CompanyId = company.Id, Status = CompanyCreationStatus.Success };
            }
            return new CompanyAddResponse() { Status = CompanyCreationStatus.NonUnique };
        }

        public bool IsCompanyExists(string name)
        {
            var dbContext = dbFactory.CreateDbContext();

            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            return IsCompanyExists(unitOfWork, name);
        }

        private static bool IsCompanyExists(IUnitOfWork unitOfWork, string name)
        {
            return unitOfWork.StructureUnitRepository.Query(c => c.UnitType == UnitType.Company && c.Name.Equals(name,StringComparison.InvariantCultureIgnoreCase)).Any();
        }
    }
}