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
			string result;
			switch (status)
			{
				case LicenseLicenseStatus.MachineNotFound:
					result = "Machine not found";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (machinesStatus)
			{
				case LicenseMachinesStatus.LicenseNotFound:
					result = "License not found";
					break;
				case LicenseMachinesStatus.LicenseCountExceeded:
					result = "License count exceeded";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case UnLicenseMachinesStatus.LicenseNotFound:
					result = "License not found";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case UnLicenseLicensesStatus.MachineNotFound:
					result = "Machine not found";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case LicenseSoftwareStatus.SoftwareOnMachineNotFound:
					result = "Software on machine not found";
					break;
				case LicenseSoftwareStatus.SoftwareNotFound:
					result = "Software not found";
					break;
				case LicenseSoftwareStatus.MachineNotFound:
					result = "Machine not found";
					break;
				case LicenseSoftwareStatus.SoftwareIsLinked:
					result = "Software on machine allready linked to license";
					break;
				case LicenseSoftwareStatus.LicenseNotFound:
					result = "License not found";
					break;
				case LicenseSoftwareStatus.LicenseNotForSoftware:
					result = "License cannot be linked to these software";
					break;
				case LicenseSoftwareStatus.LicenseCountExceeded:
					result = "License count exceeded";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case UnLicenseSoftwareStatus.SoftwareOnMachineNotFound:
					result = "Software on machine not found";
					break;
				case UnLicenseSoftwareStatus.SoftwareNotFound:
					result = "Software not found";
					break;
				case UnLicenseSoftwareStatus.MachineNotFound:
					result = "Machine not found";
					break;
				case UnLicenseSoftwareStatus.SoftwareIsNotLinked:
					result = "Software on machine is not linked to license";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case SaveLicenseRequestStatus.SoftwareOnMachineNotFound:
					result = "Software on machine not found";
					break;
				case SaveLicenseRequestStatus.SoftwareNotFound:
					result = "Software not found";
					break;
				case SaveLicenseRequestStatus.MachineNotFound:
					result = "Machine not found";
					break;
				case SaveLicenseRequestStatus.UserNotFound:
					result = "User not found";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case UpdateLicenseRequestStatus.NotExist:
					result = "License request not found";
					break;
				case UpdateLicenseRequestStatus.WrongStatus:
					result = "Wrong status";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case SendLicenseRequestStatus.NotExist:
					result = "License request not found";
					break;
				case SendLicenseRequestStatus.WrongStatus:
					result = "Wrong status";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case CreateLicenseBasedOnLicenseRequestStatus.NotExist:
					result = "License request not found";
					break;
				case CreateLicenseBasedOnLicenseRequestStatus.WrongStatus:
					result = "Wrong status";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case ArchiveLicenseRequestStatus.NotExist:
					result = "License request not found";
					break;
				case ArchiveLicenseRequestStatus.WrongStatus:
					result = "Wrong status";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case ObservableCreationStatus.SoftwareNotFound:
					result = "Software not found";
					break;
				case ObservableCreationStatus.ObservableSoftwareExist:
					result = "Software is already linked to process";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case ObservableRemoveStatus.MachineObservableProcessNotFound:
					result = "Observable process not appended to machine";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case LicenseUpdateStatus.LinkedToRemovedSoftware:
					result =
						"Cannot update. You try to delete licensed software from license. These software currently licensed. Please, remove license from software installations.";
					break;
				default:
					result = "Unknown error";
					break;
			}
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
			string result;
			switch (status)
			{
				case ValidateUserStatus.Fail:
					result = "The user name or password provided is incorrect";
					break;
				case ValidateUserStatus.Locked:
					result = "Account is locked. Please, contact your administrator";
					break;
				case ValidateUserStatus.NotApproved:
					result = "Please, approve your account";
					break;
				default:
					result = "The user name or password provided is incorrect";
					break;
			}
			return result;
		}
	}
}