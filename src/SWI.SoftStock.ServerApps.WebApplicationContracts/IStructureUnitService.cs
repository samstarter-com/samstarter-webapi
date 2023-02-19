using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.CreateAndAdd;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByName;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetCompanyIdByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.StructureUnitService.GetStructureUnitModels;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IStructureUnitService
	{
        #region structure unit query

        Task<StructureUnitModel> GetByUniqueId(Guid uniqueId);
        Task<Guid?> GetParentUniqueId(Guid uniqueId);

        Task<GetStructureUnitModelsResponse> GetStructureUnitModels(Guid userId, Guid? selectedId, string[] roles);

        Task<GetCompanyIdByNameResponse> GetCompanyIdByName(string companyName);

        Task<int> GetIdByUniqueId(Guid structureUnitId);

        Task<IEnumerable<Guid>> GetStructureUnitsGuid(Guid userId, string[] roles);

        GetCompanyIdByStructureUnitIdResponse GetCompanyIdByStructureUnitId(Guid structureUnitId);

        #endregion

        #region structure unit command

        Task<StructureUnitDeleteStatus> DeleteByUniqueId(Guid uniqueId);

		Task<CreateAndAddResponse> CreateAndAdd(StructureUnitModel model, Guid parentId);

		Task<StructureUnitUpdateStatus> Update(StructureUnitModel model);

		#endregion
	}
}