namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId
{
    using System;

    public class GetSoftwaresByMachineIdRequest : GetItemsRequest
    {
        public Guid MachineId { get; set; }

        public int FilterType { get; set; }
    }
}