namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.RegisterCompany
{
    public enum RegisterCompanyStatus
    {
        Success = 0,

        //
        // Summary:
        //     The user name was not found in the database.
        InvalidUserName = 1,
        //
        // Summary:
        //     The password is not formatted correctly.
        InvalidPassword = 2,   
        //
        // Summary:
        //     The e-mail address is not formatted correctly.
        InvalidEmail = 5,
        //
        // Summary:
        //     The user name already exists in the database for the application.
        DuplicateUserName = 6,
        //
        // Summary:
        //     The e-mail address already exists in the database for the application.
        DuplicateEmail = 7,
      
    
      
        //
        // Summary:
        //     The provider returned an error that is not described by other System.Web.Security.MembershipCreateStatus
        //     enumeration values.
        ProviderError = 11,
        //
        // Summary:
        //     The company already exists in the database for the application.
        DuplicateCompany = 12,     

        UnknownError = 100
    }
}