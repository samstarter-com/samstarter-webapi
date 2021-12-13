namespace SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetByUserId
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;
    using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

    public class GetByUserIdResponse
    {
        public GetByUserIdStatus Status { get; set; }
        public PersonalLicenseRequestCollection Model { get; set; }
    }
}