using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetAvailableLicensesBySoftwareId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsage;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetLicenseUsageList;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.Add;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface ILicenseService
	{
        #region license-structure unit command

	    Task<LicenseLinkToStructureUnitStatus> LinkToStructureUnitAsync(Guid licenseId, Guid structureUnitId);

		#endregion

		IEnumerable<DropDownItemModel> GetLicenseTypes();

		#region license-software query

		GetAvailableLicensesBySoftwareIdResponse GetAvailableLicensesBySoftwareId(
			GetAvailableLicensesBySoftwareIdRequest request);

		#endregion

		#region license-structure unit query

		Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request);

		GetStructureUnitIdResponse GetStructureUnitId(GetStructureUnitIdRequest request);

		#endregion

		#region license query

		LicenseModelEx GetLicenseModelExById(Guid licenseId);
		LicenseModel GetLicenseModelById(Guid licenseId);
		DocumentModelEx GetDocumentById(Guid id);

		#endregion

		#region license command

        Task<LicenseAddResponse> AddAsync(LicenseModelEx model);
        Task<LicenseUpdateStatus> UpdateAsync(LicenseModelEx model);
		LicenseDeleteStatus DeleteById(Guid licenseId);

		#endregion

		#region license usage query

		GetLicenseUsageResponse GetLicenseUsage(GetLicenseUsageRequest request);
		GetLicenseUsageListResponse GetLicenseUsageList(GetLicenseUsageListRequest request);

		#endregion
	}
}