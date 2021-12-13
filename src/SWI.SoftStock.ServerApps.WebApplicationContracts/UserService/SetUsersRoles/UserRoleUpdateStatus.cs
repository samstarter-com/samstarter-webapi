namespace SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles
{
    public enum UserRoleUpdateStatus
    {
        Success = 0,

        UserNotExist = 1,

        RunTime = 2,

        StructureUnitNotExist = 3,

        IsLastAdmin = 4
    }
}