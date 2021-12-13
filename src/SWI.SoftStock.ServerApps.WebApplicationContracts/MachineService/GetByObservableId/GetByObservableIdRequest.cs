namespace SWI.SoftStock.ServerApps.WebApplicationContracts.MachineService.GetByObservableId
{
    using System;

    public class GetByObservableIdRequest : GetItemsRequest
    {
        public Guid ObservableId { get; set; }
    }
}