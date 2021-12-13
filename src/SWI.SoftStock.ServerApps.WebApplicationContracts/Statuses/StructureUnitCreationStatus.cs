namespace SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses
{
    public enum StructureUnitCreationStatus
    {
        // Summary:
        //     The structure unit was successfully created.
        Success = 0,

        /// <summary>
        ///     Parent structure unit not found by uniqueId
        /// </summary>
        ParentNotFound = 1,

        NonUnique = 2,

        RunTime = 3
    }

    public enum CompanyRegistrationStatus
    {
        // Summary:
        //     The structure unit was successfully created.
        Success = 0,

        NonUnique = 1,

        RunTime = 2
    }

    public enum StructureUnitUpdateStatus
    {
        // The structure unit was successfully created.
        Success = 0,

        NonUnique = 1,

        NotExist = 2,

        ParentStructureUnitIsSame = 3
    }



    public enum UserUpdateStatus
    {
        Success = 0,

        NonUnique = 1,

        NotExist = 2,

        EmailNonUnique = 3
    }

    public enum MachineLinkToStructureUnitStatus
    {
        Success = 0
    }

    public enum MachineLinkToUserStatus
    {
        Success = 0
    }

    public enum LicenseUpdateStatus
    {
        Success = 0,

        LinkedToRemovedSoftware = 1,

        NotExist = 2
    }

    public enum LicenseDeleteStatus
    {
        Success = 0,

        LicenseAttachedToMachine = 1,

        NotExist = 2,

        UnknownError = 100
    }

    public enum SaveLicenseRequestStatus
    {
        Success = 0,

        SoftwareOnMachineNotFound = 1,

        SoftwareNotFound = 2,

        MachineNotFound = 3,

        UserNotFound = 4,

        ManagerNotFound = 5
    }

    public enum UpdateLicenseRequestStatus
    {
        Success = 0,

        NotExist = 1,

        WrongStatus = 2
    }

    public enum AnswerPersonalLicenseRequestStatus
    {
        Success = 0,

        NotExist = 1,

        WrongStatus = 2
    }

    public enum SendLicenseRequestStatus
    {
        Success = 0,

        NotExist = 1,

        WrongStatus = 2
    }

    public enum ArchiveLicenseRequestStatus
    {
        Success = 0,

        NotExist = 1,

        WrongStatus = 2
    }

    public enum ObservableCreationStatus
    {
        Success = 0,

        SoftwareNotFound = 1,

        ObservableSoftwareExist = 2
    }

    public enum ObservableAppendStatus
    {
        Success = 0,

        ObservableNotFound = 1,

        MachineNotFound = 2,

        SoftwareNotInstalledOnMachine = 3,

        AlreadyAppended = 4
    }

    public enum ObservableRemoveStatus
    {
        Success = 0,

        MachineObservableProcessNotFound = 1
    }

    public enum ObservableDeleteStatus
    {
        Success = 0,

        AppendedToMachine = 1,

        NotExist = 2,

        UnknownError = 100
    }

    public enum LicenseLinkToStructureUnitStatus
    {
        Success = 0,

        NotExist = 1,

        StructureUnitNotExist = 2
    }

    public enum MachineDeleteStatus
    {
        Success = 0,

        NotExist = 1,

        UnknownError = 100
    }

    public enum MachineDisableStatus
    {
        Success = 0,

        NotExist = 1,

        UnknownError = 100
    }

    public enum MachineEnableStatus
    {
        Success = 0,

        NotExist = 1,

        UnknownError = 100
    }













}