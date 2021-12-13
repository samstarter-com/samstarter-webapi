using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany;

namespace SWI.SoftStock.WebApi.Mapper
{
    static class RegisterCompanyStatusMessage
    {
        public static string GetErrorMessage(RegisterCompanyStatus status)
        {
            string result;
            switch (status)
            {
                case RegisterCompanyStatus.DuplicateUserName:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_DuplicateUserName;
                    break;
                case RegisterCompanyStatus.DuplicateEmail:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_DuplicateEmail;
                    break;
                case RegisterCompanyStatus.DuplicateCompany:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_DuplicateCompany;
                    break;
                case RegisterCompanyStatus.InvalidPassword:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_InvalidPassword;
                    break;
                case RegisterCompanyStatus.InvalidEmail:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_InvalidEmail;
                    break;              
                case RegisterCompanyStatus.InvalidUserName:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_InvalidUserName;
                    break;
                case RegisterCompanyStatus.ProviderError:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_ProviderError;
                    break;           
                default:
                    result = Resources.RegisterCompanyStatus.RegisterCompanyStatus_Default;
                    break;
            }
            return result;
        }
    }
}
