namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseService.GetAvailableLicensesBySoftwareId
{
    using System;
    using System.Collections.Generic;

    public class GetAvailableLicensesBySoftwareIdRequest : GetItemsRequest
    {
        public IEnumerable<Guid> SuGuids { get; set; }

        public Guid SoftwareId { get; set; }
    }
}