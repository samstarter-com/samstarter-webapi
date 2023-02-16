using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.CreateLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequestCount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface ILicenseRequestService
    {
        #region license request query
        Task<NewLicenseRequestResponse> GetNewLicenseRequest(NewLicenseRequestRequest request);
        Task<LicenseRequestModel> GetLicenseRequestModelByIdAsync(Guid licenseRequestId);
        GetByStructureUnitIdResponse GetByStructureUnitId(GetByStructureUnitIdRequest request);
        Task<LicenseRequestDocumentModelEx> GetDocumentById(Guid id);
        Task<GetNewLicenseRequestCountResponse> GetNewLicenseRequestCount(GetNewLicenseRequestCountRequest request);

        #endregion

        #region license request commnd

        Task<Tuple<Guid?, SaveLicenseRequestStatus>> Add(NewLicenseRequestModel model, Guid managerId, bool sending);
        Task<UpdateLicenseRequestStatus> Update(LicenseRequestModel model, bool sending);
        Task<SendLicenseRequestStatus> SendToUser(Guid licenseRequestId);
        Task<CreateLicenseBasedOnLicenseRequestResponse> CreateLicenseAsync(CreateLicenseBasedOnLicenseRequestRequest request);
        Task ReceivedAsync(Guid id);
        Task<ArchiveLicenseRequestStatus> Archive(Guid licenseRequestId);

        #endregion
    }
}