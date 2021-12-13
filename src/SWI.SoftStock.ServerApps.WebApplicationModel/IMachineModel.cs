using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public interface IMachineModel
    {

        Guid? UserId { get; set; }

        Guid MachineId { get; set; }

        Guid? StructureUnitId { get; set; }

        string OperationSystemName { get; set; }

        string StructureUnitName { get; set; }

        string Name { get; set; }

        string DomainUserName { get; set; }

        string DomainUserDomainName { get; set; }

        string LinkedUserName { get; set; }

        DateTime CreatedOn { get; set; }

        DateTime LastActivity { get; set; }

        int TotalSoftwareCount { get; set; }

        int LicensedSoftwareCount { get; set; }

        int UnLicensedSoftwareCount { get; set; }

        int ExpiredLicensedSoftwareCount { get; set; }

        bool Enabled { get; set; }
    }
}