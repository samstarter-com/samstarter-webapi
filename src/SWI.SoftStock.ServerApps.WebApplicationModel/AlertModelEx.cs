using System;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class AlertModelEx
    {
        public DateTime AlertDateTime { get; set; }
        public string[] AlertUsersId { get; set; }
        public UserModel[] AlertUsers { get; set; }
        public string AlertText { get; set; }

        public Guid? Id { get; set; }
    }
}