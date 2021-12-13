namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest
{
    using System;

    public class NewLicenseRequestRequest
    {
        public Guid MachineId { get; set; }

        public Guid SoftwareId { get; set; }
    }
}