using System;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses
{
    [Flags]
    public enum StructureUnitDeleteStatus
    {
        None = 0,
        HasChildStructureUnit = 1 << 0,
        HasMachine = 1 << 1,
        HasUser = 1 << 2,
        UnknownError = 1 << 10,
    }
}