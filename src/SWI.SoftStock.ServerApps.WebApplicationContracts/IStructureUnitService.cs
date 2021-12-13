using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IStructureUnitService
	{
		#region structure unit query

		StructureUnitModel GetByUniqueId(Guid uniqueId);
		Guid? GetParentUniqueId(Guid uniqueId);

		IEnumerable<StructureUnitTreeItemModel> GetStructureUnitModels(Guid userId, Guid? selectedId, string[] roles,
			out StructureUnitModel selectedStructureUnit);

		Tuple<int, Guid> GetCompanyIdByName(string companyName);

		int GetIdByUniqueId(Guid structureUnitId);

		IEnumerable<Guid> GetStructureUnitsGuid(Guid userId, string[] roles);

        Tuple<int, Guid> GetCompanyIdByStructureUnitId(Guid structureUnitId);

        #endregion

        #region structure unit command

        StructureUnitDeleteStatus DeleteByUniqueId(Guid uniqueId);

		Guid? CreateAndAdd(StructureUnitModel model, Guid parentId, out StructureUnitCreationStatus status);

		Task<StructureUnitUpdateStatus> Update(StructureUnitModel model);

		#endregion
	}
}