using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.CreateLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseLicense;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.LicenseSoftware;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseLicenses;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseMachines;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicensingService.UnLicenseSoftware;
using SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ValidateUser;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.ServerApps.WebApplicationServices.Mappers
{
    /// <summary>
    ///     English
    /// </summary>
    public static class LicenseLicenseStatusEn
	{
		public static string GetErrorMessage(LicenseLicenseStatus status)
		{
            string result = status switch
            {
                LicenseLicenseStatus.MachineNotFound => "Machine not found",
                _ => "Unknown error",
            };
            return result;
		}
	}

    /// <summary>
	///     English
	/// </summary>
	public static class LicenseMachinesStatusStatusEn
	{
		public static string GetErrorMessage(LicenseMachinesStatus machinesStatus)
		{
            string result = machinesStatus switch
            {
                LicenseMachinesStatus.LicenseNotFound => "License not found",
                LicenseMachinesStatus.LicenseCountExceeded => "License count exceeded",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class UnLicenseMachinesStatusEn
	{
		public static string GetErrorMessage(UnLicenseMachinesStatus status)
		{
            string result = status switch
            {
                UnLicenseMachinesStatus.LicenseNotFound => "License not found",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class UnLicenseLicensesStatusEn
	{
		public static string GetErrorMessage(UnLicenseLicensesStatus status)
		{
            string result = status switch
            {
                UnLicenseLicensesStatus.MachineNotFound => "Machine not found",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class LicenseSoftwareStatusEn
	{
		public static string GetErrorMessage(LicenseSoftwareStatus status)
		{
            string result = status switch
            {
                LicenseSoftwareStatus.SoftwareOnMachineNotFound => "Software on machine not found",
                LicenseSoftwareStatus.SoftwareNotFound => "Software not found",
                LicenseSoftwareStatus.MachineNotFound => "Machine not found",
                LicenseSoftwareStatus.SoftwareIsLinked => "Software on machine allready linked to license",
                LicenseSoftwareStatus.LicenseNotFound => "License not found",
                LicenseSoftwareStatus.LicenseNotForSoftware => "License cannot be linked to these software",
                LicenseSoftwareStatus.LicenseCountExceeded => "License count exceeded",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class UnLicenseSoftwareStatusEn
	{
		public static string GetErrorMessage(UnLicenseSoftwareStatus status)
		{
            string result = status switch
            {
                UnLicenseSoftwareStatus.SoftwareOnMachineNotFound => "Software on machine not found",
                UnLicenseSoftwareStatus.SoftwareNotFound => "Software not found",
                UnLicenseSoftwareStatus.MachineNotFound => "Machine not found",
                UnLicenseSoftwareStatus.SoftwareIsNotLinked => "Software on machine is not linked to license",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class SaveLicenseRequestStatusEn
	{
		public static string GetErrorMessage(SaveLicenseRequestStatus status)
		{
            string result = status switch
            {
                SaveLicenseRequestStatus.SoftwareOnMachineNotFound => "Software on machine not found",
                SaveLicenseRequestStatus.SoftwareNotFound => "Software not found",
                SaveLicenseRequestStatus.MachineNotFound => "Machine not found",
                SaveLicenseRequestStatus.UserNotFound => "User not found",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class UpdateLicenseRequestStatusEn
	{
		public static string GetErrorMessage(UpdateLicenseRequestStatus status)
		{
            string result = status switch
            {
                UpdateLicenseRequestStatus.NotExist => "License request not found",
                UpdateLicenseRequestStatus.WrongStatus => "Wrong status",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class SendLicenseRequestStatusEn
	{
		public static string GetErrorMessage(SendLicenseRequestStatus status)
		{
            string result = status switch
            {
                SendLicenseRequestStatus.NotExist => "License request not found",
                SendLicenseRequestStatus.WrongStatus => "Wrong status",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class CreateLicenseBasedOnLicenseRequestStatusEn
	{
		public static string GetErrorMessage(CreateLicenseBasedOnLicenseRequestStatus status)
		{
            string result = status switch
            {
                CreateLicenseBasedOnLicenseRequestStatus.NotExist => "License request not found",
                CreateLicenseBasedOnLicenseRequestStatus.WrongStatus => "Wrong status",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class ArchiveLicenseRequestStatusEn
	{
		public static string GetErrorMessage(ArchiveLicenseRequestStatus status)
		{
            string result = status switch
            {
                ArchiveLicenseRequestStatus.NotExist => "License request not found",
                ArchiveLicenseRequestStatus.WrongStatus => "Wrong status",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class ObservableCreationStatusEn
	{
		public static string GetErrorMessage(ObservableCreationStatus status)
		{
            string result = status switch
            {
                ObservableCreationStatus.SoftwareNotFound => "Software not found",
                ObservableCreationStatus.ObservableSoftwareExist => "Software is already linked to process",
                _ => "Unknown error",
            };
            return result;
		}
	}

    /// <summary>
	///     English
	/// </summary>
	public static class ObservableRemoveStatusEn
	{
		public static string GetErrorMessage(ObservableRemoveStatus status)
		{
            string result = status switch
            {
                ObservableRemoveStatus.MachineObservableProcessNotFound => "Observable process not appended to machine",
                _ => "Unknown error",
            };
            return result;
		}
	}

	/// <summary>
	///     English
	/// </summary>
	public static class LicenseUpdateStatusEn
	{
		public static string GetErrorMessage(LicenseUpdateStatus status)
		{
            string result = status switch
            {
                LicenseUpdateStatus.LinkedToRemovedSoftware => "Cannot update. You try to delete licensed software from license. These software currently licensed. Please, remove license from software installations.",
                _ => "Unknown error",
            };
            return result;
		}
	}	

	/// <summary>
	///     English
	/// </summary>
	public static class ValidateUserStatusEn
	{
		public static string GetErrorMessage(ValidateUserStatus status)
		{
            string result = status switch
            {
                ValidateUserStatus.Fail => "The user name or password provided is incorrect",
                ValidateUserStatus.Locked => "Account is locked. Please, contact your administrator",
                ValidateUserStatus.NotApproved => "Please, approve your account",
                _ => "The user name or password provided is incorrect",
            };
            return result;
		}
	}
}