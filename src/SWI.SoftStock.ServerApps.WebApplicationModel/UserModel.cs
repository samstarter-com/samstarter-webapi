using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class UserModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string StructureUnitName { get; set; }
    }
}