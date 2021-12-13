using SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.UserUnLock;

namespace SWI.SoftStock.WebApi.Mapper
{
    public static class UserUnLockStatusMessage
    {
        public static string GetErrorMessage(UserUnLockStatus status)
        {
            string result;
            switch (status)
            {
                case UserUnLockStatus.UserNotExist:
                    result = Resources.UserUnLockStatus.UserNotExist;
                    break;

                case UserUnLockStatus.UnknownError:
                    result = Resources.UserUnLockStatus.UnknownError;
                    break;
                default:
                    result = Resources.UserUnLockStatus.Default;
                    break;
            }
            return result;
        }
    }
}
