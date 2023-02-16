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

		Task<IEnumerable<DropDownItemModel>> GetLicenseTypes();

		#region license-software query

		Task<GetAvailableLicensesBySoftwareIdResponse> GetAvailableLicensesBySoftwareId(
			GetAvailableLicensesBySoftwareIdRequest request);

		#endregion

		#region license-structure unit query

		Task<GetByStructureUnitIdResponse> GetByStructureUnitIdAsync(GetByStructureUnitIdRequest request);

		Task<GetStructureUnitIdResponse> GetStructureUnitId(GetStructureUnitIdRequest request);

		#endregion

		#region license query

		Task<LicenseModelEx> GetLicenseModelExById(Guid licenseId);
		Task<LicenseModel> GetLicenseModelById(Guid licenseId);
		Task<DocumentModelEx> GetDocumentById(Guid id);

		#endregion

		#region license command

        Task<LicenseAddResponse> AddAsync(LicenseModelEx model);
        Task<LicenseUpdateStatus> UpdateAsync(LicenseModelEx model);
		Task<LicenseDeleteStatus> DeleteById(Guid licenseId);

		#endregion

		#region license usage query

		Task<GetLicenseUsageResponse> GetLicenseUsage(GetLicenseUsageRequest request);
		Task<GetLicenseUsageListResponse> GetLicenseUsageList(GetLicenseUsageListRequest request);

		#endregion
	}
}