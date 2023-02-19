using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.ObservableService.Append
{
    public class AppendResponse
    {

        public Guid? ObservableId { get; set; }
        public ObservableAppendStatus Status { get; set; }
    }
}
