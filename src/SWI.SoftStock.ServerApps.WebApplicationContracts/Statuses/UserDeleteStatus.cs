namespace SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses
{
    public enum  UserDeleteStatus
    {
        None = 0,
        IsInRole = 1,
        HasLinkedMachine = 2,
        HasLicenseAlert = 3,
        UnknownError = 100,
    }
}