namespace SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetByUserId
{
    using System;
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class GetByUserIdRequest : GetItemsRequest
    {
        public Guid UserId { get; set; }
        public PersonalLicenseRequestStatus Status { get; set; }
    }
}