namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System.ComponentModel;

    public class AccountModel
    {
        [ReadOnly(true)]
        [DisplayName("Account")]
        public string AccountName { get; set; }

        [ReadOnly(true)]
        [DisplayName("Machine count")]
        public int MachineCount { get; set; }

        [ReadOnly(true)]
        [DisplayName("Registerd machine")]
        public int InstalledMachineCount { get; set; }

        [ReadOnly(true)]
        [DisplayName("Available machine registration")]
        public int AvailableMachineCount { get; set; }
    }
}
