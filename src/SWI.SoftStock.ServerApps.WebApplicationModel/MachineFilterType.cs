using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    [Flags]
    public enum MachineFilterType
    {
        None = 0, // 0

        Enabled = 1 << 0, //1

        Disabled = 1 << 1  //2
    }
}