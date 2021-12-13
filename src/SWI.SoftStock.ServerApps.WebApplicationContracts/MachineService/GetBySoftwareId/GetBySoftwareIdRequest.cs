namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetBySoftwareId
{
    using System;
    using System.Collections.Generic;

    public class GetBySoftwareIdRequest : GetItemsRequest
    {
        public Guid SoftwareId { get; set; }

        public IEnumerable<Guid> SuIds { get; set; }

        public int FilterType { get; set; }
    }
}