using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.Add
{
    public class AddResponse
    {
        public Guid? ObservableId { get; set; }
        public ObservableCreationStatus Status { get; set; }
    }
}
