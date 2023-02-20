using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class PersonalMachineService : IPersonalMachineService
    {
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public PersonalMachineService(IDbContextFactory<MainDbContext> dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<GetByUserIdResponse> GetByUserIdAsync(GetByUserIdRequest request)
        {
            var response = new GetByUserIdResponse
            {
                Model = new WebApplicationModel.Collections.PersonalMachineCollection(request.Ordering)
            };
            var dbContext = dbFactory.CreateDbContext();
            IEnumerable<PersonalMachineModel> items;
            int totalRecords;
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var query = unitOfWork.MachineRepository.Query(m => m.CurrentUserId == request.UserId && !m.IsDisabled);
                totalRecords = await query.CountAsync();
                var machines = await query.Include(m=>m.MachineSoftwaresReadOnly).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize).ToArrayAsync();
                items = machines.Select(MapperFromModelToView.MapToPersonalMachineModel);
            }
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByUserIdStatus.Success;
            return response;
        }      
    }
}