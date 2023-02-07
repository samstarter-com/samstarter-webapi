namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public interface ISoftwareModel : ISimpleSoftwareModel
    {
        int TotalInstallationCount { get; set; }

        int LicensedInstallationCount { get; set; }

        int UnLicensedInstallationCount { get; set; }
    }
}