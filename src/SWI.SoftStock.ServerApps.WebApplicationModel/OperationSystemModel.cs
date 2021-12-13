namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class OperationSystemModel
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Architecture { get; set; }
        public string BuildNumber { get; set; }

        public string BootMode { get; set; }
        public string LogicalDrives { get; set; }
        public string SerialNumber { get; set; }
        public string SystemDirectory { get; set; }
    }
}