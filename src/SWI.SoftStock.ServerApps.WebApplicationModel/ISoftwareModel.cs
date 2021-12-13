namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    public interface ISimpleSoftwareModel
    {

        Guid SoftwareId { get; set; }

        string Name { get; set; }

        string PublisherName { get; set; }

        string Version { get; set; }

        string SystemComponent { get; set; }

        string WindowsInstaller { get; set; }

        string ReleaseType { get; set; }
    }

    public interface ISoftwareModel : ISimpleSoftwareModel
    {
        /// <summary>
        /// ���������� �����������
        /// </summary>
        int TotalInstallationCount { get; set; }

        /// <summary>
        /// ���������� ��������������� �����������
        /// </summary>
        int LicensedInstallationCount { get; set; }

        /// <summary>
        /// ���������� �� ��������������� �����������
        /// </summary>
        int UnLicensedInstallationCount { get; set; }
    }
}