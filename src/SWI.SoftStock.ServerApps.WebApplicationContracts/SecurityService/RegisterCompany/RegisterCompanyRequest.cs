using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany
{
    public class RegisterCompanyRequest
    {
        public RegisterModel RegisterModel { get; set; }

        public string AccountName { get; set; }     
    }
}