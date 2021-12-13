namespace SWI.SoftStock.ServerApps.WebApplicationContracts.UserService.SetUsersRoles
{
    using System;

    public class SetUsersRolesRequest
    {
        public RoleData[] Roles { get; set; }

        public Guid StructureUnitId { get; set; }

        public Guid UserId { get; set; }
    }

    public struct RoleData
    {
        public Guid RoleId { get; set; }

        public bool IsInRole { get; set; }
    }
}