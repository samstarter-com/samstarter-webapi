using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetById;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface ISoftwareService
	{

		#region software query

        Task<GetByIdResponse> GetByIdAsync(GetByIdRequest request);

        Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request);

		SoftwareCollection GetForAutocomplete(Guid value, string contains, bool? includeSubUnits = false);

		Task<GetSoftwaresByMachineIdResponse> GetByMachineId(GetSoftwaresByMachineIdRequest request);

		IEnumerable<LicenseFilterTypeModel> GetSoftwareTypes();

		#endregion
	}
}