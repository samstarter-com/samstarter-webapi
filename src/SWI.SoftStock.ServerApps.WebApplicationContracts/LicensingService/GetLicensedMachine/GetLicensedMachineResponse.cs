using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetLicensedMachine
{
	public class GetLicensedMachineResponse
    {
        public GetLicensedMachineStatus Status { get; set; }
        public LicenseMachineCollection Model { get; set; }
    }
}