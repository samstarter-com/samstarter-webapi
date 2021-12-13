using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.WebApi.Mapper
{
    static class UserDeleteStatusMessage
    {
        public static string GetErrorMessage(UserDeleteStatus status)
        {
            string result;
            switch (status)
            {
                case UserDeleteStatus.IsInRole:
                    result = Resources.UserDeleteStatus.IsInRole;
                    break;
                case UserDeleteStatus.HasLinkedMachine:
                    result = Resources.UserDeleteStatus.HasLinkedMachine;
                    break;
                case UserDeleteStatus.HasLicenseAlert:
                    result = Resources.UserDeleteStatus.HasLicenseAlert;
                    break;
                case UserDeleteStatus.UnknownError:
                    result = Resources.UserDeleteStatus.UnknownError;
                    break;
                default:
                    result = Resources.UserDeleteStatus.UnknownError;
                    break;
            }
            return result;
        }
    }
}
