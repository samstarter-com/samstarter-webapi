using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    // todo на удаление - проверить используется ли - заменить на SoftwareMachineCollection

    public class SoftwareMachinesModel
    {
        public IEnumerable<MachineModel> Machines { get; set; }
        public string SoftwareName { get; set; }
    }
}