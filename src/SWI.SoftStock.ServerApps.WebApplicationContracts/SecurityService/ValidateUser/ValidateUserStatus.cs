namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser
{
    public enum ValidateUserStatus
    {
        Success = 0,
        Fail = 1,
        NotApproved = 2,
        Locked = 3,
        UnknownError = 100
    }
}