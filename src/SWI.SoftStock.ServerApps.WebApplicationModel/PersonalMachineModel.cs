using SWI.SoftStock.ServerApps.WebApplicationModel.Attributes;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    public class PersonalMachineModel : BaseModel<PersonalMachineModel>
    {
        public Guid MachineId { get; set; }

        [Sort("thName")]
        public string Name { get; set; }

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
    }
}