using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetByUserId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetNewLicenseRequestCount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface IPersonalLicenseRequestService
	{
		#region personal license request query

        Task<PersonalLicenseRequestModel> GetLicenseRequestModelByIdAsync(Guid licenseRequestId);
		PersonalLicenseRequestDocumentModelEx GetDocumentById(Guid id);
        Task<GetByUserIdResponse> GetByUserIdAsync(GetByUserIdRequest request);
		Task<GetNewLicenseRequestCountResponse> GetNewLicenseRequestCount(GetNewLicenseRequestCountRequest request);

		#endregion

		#region personal license request command

		Task ReceivedAsync(Guid id);
		AnswerPersonalLicenseRequestStatus Answer(PersonalLicenseRequestAnswerModel model);

		#endregion
	}
}