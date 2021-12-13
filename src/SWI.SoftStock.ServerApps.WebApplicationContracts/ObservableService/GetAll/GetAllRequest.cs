namespace SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.GetAll
{
    using System;

    public class GetAllRequest : GetItemsRequest
    {
        public Guid CompanyId { get; set; }

        public string Prname { get; set; }

        public Guid? FilterSoftwareId { get; set; }
    }
}