namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    using System;

    [Flags]
    public enum PersonalLicenseRequestStatus
    {
        None = 0,

        New = 1 << 0,

        ViewedByUser = 1 << 1,

        Answered = 1 << 2
    }
}