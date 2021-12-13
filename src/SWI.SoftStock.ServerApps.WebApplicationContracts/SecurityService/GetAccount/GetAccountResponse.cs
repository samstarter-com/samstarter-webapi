namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.GetAccount
{
    using SWI.SoftStock.ServerApps.WebApplicationModel;

    public class GetAccountResponse
    {
        public GetAccountStatus Status { get; set; }
        public AccountModel AccountModel { get; set; }
    }
}