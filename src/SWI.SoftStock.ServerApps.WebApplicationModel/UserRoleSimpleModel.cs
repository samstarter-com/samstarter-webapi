using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class UserRoleSimpleModel
    {
        public string RoleName { get; set; }
        public Guid RoleId { get; set; }

        public string UserName { get; set; }
        public Guid UserId { get; set; }

        public Guid StructureUnitId { get; set; }
        public string StructureUnitName { get; set; }

        public bool IsInherited { get; set; }
    }
}