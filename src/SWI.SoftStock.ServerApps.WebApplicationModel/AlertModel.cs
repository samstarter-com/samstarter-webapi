namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    public class AlertModel
    {
        public DateTime AlertDateTime { get; set; }
        public string AlertText { get; set; }
        public UserModel[] AlertUsers { get; set; }
    }
}