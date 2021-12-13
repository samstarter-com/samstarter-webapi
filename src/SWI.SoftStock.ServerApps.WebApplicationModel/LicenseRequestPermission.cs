namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    [Flags]
    public enum LicenseRequestPermission
    {
        None = 0, //0
        View = 1 << 0, //1
        CreateAnswer = 1 << 1, //2
        CreateLicense = 1 << 2, //4
        MoveToArchive = 1 << 3, //8
        Update = 1 << 4 //16
    }
}