using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    // todo �� �������� - ��������� ������������ �� - �������� �� SoftwareMachineCollection

    public class SoftwareMachinesModel
    {
        public IEnumerable<MachineModel> Machines { get; set; }
        public string SoftwareName { get; set; }
    }
}