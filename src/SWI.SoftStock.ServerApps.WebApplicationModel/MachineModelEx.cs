namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///    Extended machine model
    /// </summary>
    public class MachineModelEx
    {
        public Guid MachineId { get; set; }

        /// <summary>
        ///     Id of linked user
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        ///     Login name of linked user
        /// </summary>
        public string UserName { get; set; }

        public Guid? StructureUnitId { get; set; }

        public string StructureUnitName { get; set; }

        public string Name { get; set; }

        public int MonitorCount { get; set; }

        public double MemoryTotalCapacity { get; set; }

        public string DomainUserName { get; set; }

        public string DomainUserDomainName { get; set; }

        public bool MonitorsSameDisplayFormat { get; set; }

        public int MouseButtons { get; set; }

        public string ScreenOrientation { get; set; }

        public int ProcessorCount { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public DateTime LastActivity { get; set; }

        public IEnumerable<NetworkAdapterModel> NetworkAdapters { get; set; }

        public ProcessorModel Processor { get; set; }

        public OperationSystemModel OperationSystem { get; set; }

        public bool Enabled { get; set; }

        /// <summary>
        /// Total installed software
        /// </summary>
        public int TotalSoftwareCount { get; set; }

        /// <summary>
        /// Software count with active license
        /// </summary>
        public int LicensedSoftwareCount { get; set; }

        /// <summary>
        /// Software count without license
        /// </summary>
        public int UnLicensedSoftwareCount { get; set; }

        /// <summary>
        /// Software count with expired license
        /// </summary>
        public int ExpiredLicensedSoftwareCount { get; set; }

        public IEnumerable<ObservableModel> ObservableProcesses { get; set; }
    }
}