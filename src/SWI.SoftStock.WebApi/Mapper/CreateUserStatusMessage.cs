using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.CreateUser;

namespace SWI.SoftStock.WebApi.Mapper
{
    static class CreateUserStatusMessage
    {
        public static string GetErrorMessage(CreateUserStatus status)
        {
            string result;
            switch (status)
            {
                case CreateUserStatus.DuplicateUserName:
                    return Resources.CreateUserStatus.CreateUserStatus_DuplicateUserName;
                case CreateUserStatus.DuplicateEmail:
                    result =
                       Resources.CreateUserStatus.CreateUserStatus_DuplicateEmail;
                    break;
                case CreateUserStatus.DuplicateCompany:
                    result = Resources.CreateUserStatus.CreateUserStatus_DuplicateCompany;
                    break;
                case CreateUserStatus.InvalidPassword:
                    result = Resources.CreateUserStatus.CreateUserStatus_InvalidPassword;
                    break;
                case CreateUserStatus.InvalidEmail:
                    result = Resources.CreateUserStatus.CreateUserStatus_InvalidEmail;
                    break;
                case CreateUserStatus.InvalidAnswer:
                    result = Resources.CreateUserStatus.CreateUserStatus_InvalidAnswer;
                    break;
                case CreateUserStatus.InvalidQuestion:
                    result = Resources.CreateUserStatus.CreateUserStatus_InvalidQuestion;
                    break;
                case CreateUserStatus.InvalidUserName:
                    result = Resources.CreateUserStatus.CreateUserStatus_InvalidUserName;
                    break;
                case CreateUserStatus.ProviderError:
                    result =
                       Resources.CreateUserStatus.CreateUserStatus_ProviderError;
                    break;
                case CreateUserStatus.UserRejected:
                    result =
                        Resources.CreateUserStatus.CreateUserStatus_UserRejected;
                    break;
                default:
                    result =
                         Resources.CreateUserStatus.CreateUserStatus_Default;
                    break;
            }
            return result;
        }
    }
}
