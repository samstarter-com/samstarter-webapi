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
            string result;
            switch (status)
            {
                case GetSoftwaresByMachineIdStatus.MachineNotFound:
                    result = "Machine not found";
                    break;
                case GetSoftwaresByMachineIdStatus.MachineIsDisabled:
                    result = "Machine is deleted";
                    break;
                default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }
}