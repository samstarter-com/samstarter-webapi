using System;
using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    [Sortable]
	public class MachineModel : BaseModel<MachineModel>, IMachineModel
    {
        public Guid? UserId { get; set; }

        public Guid MachineId { get; set; }

        public Guid? StructureUnitId { get; set; }

        [Sort("thOperationSystemName")]
        public string OperationSystemName { get; set; }

        [Sort("thStructureUnitName")]
        public string StructureUnitName { get; set; }

        [Sort("thName")]
        public string Name { get; set; }

        [Sort("thDomainUserName")]
        public string DomainUserName { get; set; }

        [Sort("thDomainUserDomainName")]
        public string DomainUserDomainName { get; set; }

        [Sort("thLinkedUserName")]
        public string LinkedUserName { get; set; }

        [Sort("thCreatedOn")]
        public DateTime CreatedOn { get; set; }

        [Sort("thLastActivity")]
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Total count of installed software
        /// </summary>
        [Sort("thTotalSoftwareCount")]
        public int TotalSoftwareCount { get; set; }

        /// <summary>
        /// Count of licensed software by active licenses
        /// </summary>
        [Sort("thLicensedSoftwareCount")]
        public int LicensedSoftwareCount { get; set; }

        /// <summary>
        /// Count of unlicensed software (without license and with expired license)
        /// </summary>
        [Sort("thUnLicensedSoftwareCount")]
        public int UnLicensedSoftwareCount { get; set; }

        /// <summary>
        /// Count of unlicensed software with expired license
        /// </summary>
        [Sort("thExpiredLicensedSoftwareCount")]
        public int ExpiredLicensedSoftwareCount { get; set; }

        [Sort("thEnabled")]
        public bool Enabled { get; set; }
    }
}