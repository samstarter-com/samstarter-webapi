using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetAvailableLicensesByMachineId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.GetLicensedMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseSoftware;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseLicenses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachine;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseSoftware;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts
{
    public interface ILicensingService
	{
		#region machine software  query

        Task<GetLicensedMachineResponse> GetLicensedMachineAsync(GetLicensedMachineRequest request);

		#endregion

		#region machine software  command


		#region license-machine query

        Task<GetAvailableLicensesByMachineIdResponse> GetAvailableLicensesByMachineIdAsync(GetAvailableLicensesByMachineIdRequest request);

		#endregion

		LicenseSoftwareResponse LicenseSoftware(LicenseSoftwareRequest softwareRequest);
        LicenseMachineResponse LicenseMachine(LicenseMachineRequest request);
		Task<LicenseMachinesResponse> LicenseMachinesAsync(LicenseMachinesRequest request);
		UnLicenseSoftwareResponse UnLicenseSoftware(UnLicenseSoftwareRequest request);
		UnLicenseMachineResponse UnLicenseMachine(UnLicenseMachineRequest request);
        Task<UnLicenseMachinesResponse> UnLicenseMachinesAsync(UnLicenseMachinesRequest request);
        Task<LicenseLicenseResponse> LicenseLicenseAsync(LicenseLicenseRequest request);
        Task<UnLicenseLicensesResponse> UnLicenseLicensesAsync(UnLicenseLicensesRequest request);

		#endregion

    }
}