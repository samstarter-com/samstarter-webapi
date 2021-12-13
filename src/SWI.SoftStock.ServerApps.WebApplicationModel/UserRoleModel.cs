namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    public class UserRoleModel
    {
        public Guid RoleId { get; set; }
        public Guid StructureUnitId { get; set; }
        public bool IsInRole { get; set; }
        public string RoleName { get; set; }
    }
}