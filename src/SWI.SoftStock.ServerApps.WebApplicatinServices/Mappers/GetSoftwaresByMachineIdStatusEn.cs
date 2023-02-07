using SWI.SoftStock.ServerApps.WebApplicationContracts.SoftwareService.GetSoftwaresByMachineId;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{

    /// <summary>
    /// English
    /// </summary>
    public static class GetSoftwaresByMachineIdStatusEn
    {
        public static string GetErrorMessage(GetSoftwaresByMachineIdStatus status)
        {
            string result = status switch
            {
                GetSoftwaresByMachineIdStatus.MachineNotFound => "Machine not found",
                GetSoftwaresByMachineIdStatus.MachineIsDisabled => "Machine is deleted",
                _ => "Unknown error",
            };
            return result;
        }
    }
}