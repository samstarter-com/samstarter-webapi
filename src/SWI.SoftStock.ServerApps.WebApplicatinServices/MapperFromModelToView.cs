using System.Linq;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity.Models;
using SWI.SoftStock.ServerApps.WebApplicationModel;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    using System;
    using System.Collections.Generic;

    public static class MapperFromModelToView
    {
        public static T MapToSoftwareModel<T>(Software software) where T : class, ISimpleSoftwareModel, new()
        {
            var result = new T();
            result.SoftwareId = software.UniqueId;
            result.Name = software.Name;
            result.PublisherName = software.Publisher != null ? software.Publisher.Name : string.Empty;
            result.Version = software.Version;
            result.SystemComponent = software.SystemComponent;
            result.WindowsInstaller = software.WindowsInstaller;
            result.ReleaseType = software.ReleaseType;
            return result;
        }


        public static T MapToSoftwareModel<T>(Software software, IEnumerable<MachineSoftware> machineSoftwares) where T : class, ISoftwareModel, new()
        {
            var nowDate = DateTime.Now.Date;
            var result = MapToSoftwareModel<T>(software);
            result.TotalInstallationCount = machineSoftwares.Count();
            result.LicensedInstallationCount =
                machineSoftwares.Count(
                    ms => ms.LicenseMachineSoftwares != null && ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted) &&
                          ms.LicenseMachineSoftwares.Single(lms => !lms.IsDeleted).LicenseSoftware.License.BeginDate <=
                          nowDate &&
                          ms.LicenseMachineSoftwares.Single(lms => !lms.IsDeleted).LicenseSoftware.License.
                              ExpirationDate > nowDate);
            result.UnLicensedInstallationCount = result.TotalInstallationCount - result.LicensedInstallationCount;
            return result;
        }

        public static T MapToSoftwareModel<T>(
            Software software,
            IGrouping<int, SoftwareCurrentLinkedStructureUnitReadOnly> values)
            where T : class, ISoftwareModel, new()
        {
            var result = MapToSoftwareModel<T>(software);
            result.TotalInstallationCount = values.Sum(v => v.SoftwaresTotalCount);
            result.LicensedInstallationCount = values.Sum(v => v.SoftwaresIsActiveCount);
            result.UnLicensedInstallationCount = values.Sum(v => v.SoftwaresUnlicensedCount);
            return result;
        }

        public static T MapToSoftwareModel<T>(Software software, SoftwareCurrentLinkedStructureUnitReadOnlyGrouped softwareGrouped)
            where T : class, ISoftwareModel, new()
        {
            var result = MapToSoftwareModel<T>(software);
            result.TotalInstallationCount = softwareGrouped.SoftwaresTotalCount;
            result.LicensedInstallationCount = softwareGrouped.SoftwaresIsActiveCount;
            result.UnLicensedInstallationCount = softwareGrouped.SoftwaresUnlicensedCount;
            return result;
        }

        public static DocumentModel MapToDocumentModel(Document document)
        {
            var result = new DocumentModel();
            result.Id = document.UniqueId;
            result.Name = document.Name;
            result.HcLocation = document.HcLocation;
            return result;
        }

        public static LicenseRequestDocumentModel MapToLicenseRequestDocumentModel(LicenseRequestDocument document)
        {
            var result = new LicenseRequestDocumentModel();
            result.Id = document.UniqueId;
            result.Name = document.Name;
            result.HcLocation = document.HcLocation;
            return result;
        }

        public static DocumentModelEx MapToDocumentModelEx(Document document, bool withContent)
        {
            var result = new DocumentModelEx();
            if (withContent)
            {
                result.Content = document.Content;
            }
            result.Id = document.UniqueId;
            result.Name = document.Name;
            result.HcLocation = document.HcLocation;
            return result;
        }

        public static AlertModel MapToLicenseAlertModel(LicenseAlert alert)
        {
            var result = new AlertModel();
            result.AlertDateTime = alert.AlertDate;
            result.AlertText = alert.Text;
            result.AlertUsers = alert.Assignees.Select(a => MapToUserModel(a.User, a.User.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit)).ToArray();
            return result;
        }

        public static AlertModelEx MapToLicenseAlertModelEx(LicenseAlert alert)
        {
            var result = new AlertModelEx();
            result.Id = alert.UniqueId;
            result.AlertDateTime = alert.AlertDate;
            result.AlertText = alert.Text;
            result.AlertUsers = alert.Assignees.Select(a => MapToUserModel(a.User, a.User.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit)).ToArray();
            return result;
        }

        public static UserModelEx MapToUserModelEx(User u)
        {
            return new UserModelEx
            {
                UserId = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                CreateDate = u.CreateDate,
                LastActivityDate = u.LastActivityDate,
                IsApproved = u.EmailConfirmed,
              //  IsLocked = u.LockoutEnabled && u.LockoutEndDateUtc > DateTime.UtcNow,
                StructureUnitId = u.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit.UniqueId,
                StructureUnitName = u.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit.ShortName
            };
        }

        public static UserModel MapToUserModel(User u, StructureUnit su)
        {
            return new UserModel
            {
                UserId = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                StructureUnitName = su.ShortName
            };
        }

        public static T MapToLicenseModel<T>(License license) where T : class, ILicenseModel, new()
        {
            var result = new T();
            result.LicenseId = license.UniqueId;
            result.Name = license.Name;
            result.LicenseTypeName = license.LicenseType.Name;
            result.LinkedSoftwares = license.LicenseSoftwares.Select(ls => MapToSoftwareModel<SoftwareModel>(ls.Software)).ToArray();
            result.BeginDate = license.BeginDate;
            result.ExpirationDate = license.ExpirationDate;
            result.Count = license.Count;
            result.AvailableCount = license.Count -
                                    license.LicenseSoftwares.SelectMany(
                                        ls => ls.LicenseMachineSoftwares.Where(lms => !lms.IsDeleted)).Select(
                                            lms => lms.MachineSoftware.MachineId).Distinct().Count();
            result.Comments = license.Comments;
            result.Documents = license.Documents.Select(MapToDocumentModel).ToArray();
            result.Alerts = license.LicenseAlerts.Select(MapToLicenseAlertModel).ToArray();
            result.StructureUnitName = license.StructureUnit.ShortName;
            result.StructureUnitId = license.StructureUnit.UniqueId;
            return result;
        }

        public static DropDownItemModel MapToLicenseTypeModel(LicenseType licenseType)
        {
            var result = new DropDownItemModel();
            result.Id = licenseType.Id;
            result.Name = licenseType.Name;
            return result;
        }

        public static LicenseModelEx MapToLicenseModelEx(License license)
        {
            var result = new LicenseModelEx();
            result.LicenseId = license.UniqueId;
            result.Name = license.Name;
            result.LicenseTypeId = license.LicenseTypeId;
            result.BeginDate = license.BeginDate;
            result.ExpirationDate = license.ExpirationDate;
            result.Comments = license.Comments;
            result.Count = license.Count;
            result.LinkedSoftwares = license.LicenseSoftwares.Select(ls => MapToSoftwareModel<SoftwareModel>(ls.Software)).ToArray();
            result.Documents = license.Documents.Select(d => MapToDocumentModelEx(d, false)).ToArray();
            result.Alerts = license.LicenseAlerts.Select(MapToLicenseAlertModelEx).ToArray();
            result.StructureUnitId = license.StructureUnit.UniqueId;
            return result;
        }

        public static ShortLicenseModel MapToShortLicenseModel(License license)
        {
            var result = new ShortLicenseModel();
            result.LicenseId = license.UniqueId;
            result.Name = license.Name;
            return result;
        }
        
        public static T MapToMachineModel<T>(Machine machine) where T : class, IMachineModel, new()
        {
            var result = new T();
            result.UserId = machine.CurrentUserId;
            result.MachineId = machine.UniqueId;
            result.OperationSystemName = machine.MachineOperationSystem?.OperationSystem?.Name;
            result.LinkedUserName = machine.CurrentUser?.UserName;
            result.DomainUserDomainName = machine.CurrentDomainUser?.DomainName;
            result.DomainUserName = machine.CurrentDomainUser?.Name;
            result.Name = machine.Name;
            result.Enabled = !machine.IsDisabled;
            result.CreatedOn = machine.CreatedOn;
            result.LastActivity = machine.LastActivityDateTime;
            result.StructureUnitId = machine.CurrentLinkedStructureUnit?.UniqueId;
            result.StructureUnitName = machine.CurrentLinkedStructureUnit != null ? machine.CurrentLinkedStructureUnit.ShortName : string.Empty;
            result.TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount;
            result.LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount;
            result.UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount;
            result.ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount;
            return result;
        }

        public static InstalledSoftwareMachineModel MapToInstalledSoftwareMachineModel(MachineSoftware ms) 
        {
           var result = MapperFromModelToView.MapToMachineModel<InstalledSoftwareMachineModel>(ms.Machine);
           result.HasLicense = ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted);
           result.LicenseName = ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted) ? ms.LicenseMachineSoftwares.FirstOrDefault(lms => !lms.IsDeleted).LicenseSoftware.License.Name : string.Empty;
           result.LicenseId = ms.LicenseMachineSoftwares.Any(lms => !lms.IsDeleted) ? ms.LicenseMachineSoftwares.FirstOrDefault(lms => !lms.IsDeleted).LicenseSoftware.License.UniqueId : (Guid?)null;
           result.InstallDate = ms.InstallDate;
           result.DiscoveryDate = ms.CreatedOn;
            return result;
        }

        public static MachineModelEx MapToMachineModelEx(Machine machine)
        {
            var result = new MachineModelEx();
            result.UserId = machine.CurrentUser?.Id;
            result.UserName = machine.CurrentUser != null ? machine.CurrentUser.UserName : string.Empty;
            result.MachineId = machine.UniqueId;
            result.DomainUserDomainName = machine.CurrentDomainUser?.DomainName;
            result.DomainUserName = machine.CurrentDomainUser != null ? machine.CurrentDomainUser.Name : null;
            result.MemoryTotalCapacity = machine.MemoryTotalCapacity;
            result.MonitorCount = machine.MonitorCount;
            result.Name = machine.Name;
            result.StructureUnitId = machine.CurrentLinkedStructureUnit != null ? (Guid?)machine.CurrentLinkedStructureUnit.UniqueId : null;
            result.StructureUnitName = machine.CurrentLinkedStructureUnit != null ? machine.CurrentLinkedStructureUnit.ShortName : string.Empty;
            result.TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount;
            result.LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount;
            result.UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount;
            result.ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount;
            result.CreatedOn = machine.CreatedOn;
            result.ModifiedOn = machine.ModifiedOn;
            result.LastActivity = machine.LastActivityDateTime;
            result.NetworkAdapters = machine.NetworkAdapters.Select(MapToNetworkAdapterModel);
            result.ObservableProcesses = machine.MachineObservedProcesses.Select(mop => mop.Observable).Select(MapToObservableModel);
            result.Processor = machine.Processor != null ? MapToProcessorModel(machine.Processor) : null;
            result.OperationSystem = machine.MachineOperationSystem != null
                ? MapToOperationSystemModel(machine.MachineOperationSystem)
                : null;
            result.Enabled = !machine.IsDisabled;
            return result;
        }

        public static NetworkAdapterModel MapToNetworkAdapterModel(NetworkAdapter networkAdapter)
        {
            var result = new NetworkAdapterModel();
            result.MacAdress = networkAdapter.MacAdress;
            result.Caption = networkAdapter.Caption;
            return result;
        }

        public static ProcessorModel MapToProcessorModel(Processor processor)
        {
            var result = new ProcessorModel();
            result.ProcessorId = processor.ProcessorId;
            result.DeviceId = processor.DeviceID;
            result.SocketDesignation = processor.SocketDesignation;
            result.Is64BitProcess = processor.Is64BitProcess;
            result.ManufacturerName = processor.Manufacturer.Name;
            return result;
        }

        public static OperationSystemModel MapToOperationSystemModel(MachineOperationSystem operationSystem)
        {
            var result = new OperationSystemModel();
            if (operationSystem.OperationSystem != null)
            {
                result.Name = operationSystem.OperationSystem.Name;
                result.Version = operationSystem.OperationSystem.Version;
                result.Architecture = operationSystem.OperationSystem.Architecture;
                result.BuildNumber = operationSystem.OperationSystem.BuildNumber;
            }
            result.BootMode = operationSystem.BootMode;
            result.LogicalDrives = operationSystem.LogicalDrives;
            result.SerialNumber = operationSystem.SerialNumber;
            result.SystemDirectory = operationSystem.SystemDirectory;
            return result;
        }

        public static LicenseRequestModel MapToManagerLicenseRequestModel(LicenseRequest licenseRequest)
        {
            var result = new LicenseRequestModel();
            result.LicenseRequestId = licenseRequest.UniqueId;
            result.MachineId = licenseRequest.Machine.UniqueId;
            result.MachineName = licenseRequest.Machine.Name;
            result.SoftwareId = licenseRequest.Software.UniqueId;
            result.SoftwareName = licenseRequest.Software.Name;
            result.SoftwarePublisher = licenseRequest.Software.Publisher != null ? licenseRequest.Software.Publisher.Name : string.Empty;
            result.Text = licenseRequest.RequestText;
            result.UserEmail = licenseRequest.UserEmail;
            result.UserId = licenseRequest.UserUserId;
            result.UserName = licenseRequest.User.UserName;
            result.CreatedOn = licenseRequest.LicenseRequestHistories.Min(lrh => lrh.StatusDateTime);
            result.ModifiedOn = licenseRequest.LicenseRequestHistories.Max(lrh => lrh.StatusDateTime);
            result.Status = GetLicenseRequestManagerStatusEn(GetManagerLicenseRequestStatus(licenseRequest.CurrentStatus));
            result.Permission = GetManagerLicenseRequestPermission(licenseRequest.CurrentStatus);
            result.Documents = licenseRequest.LicenseRequestDocuments.Select(MapToLicenseRequestDocumentModel).ToArray();
            result.UserAnswerText = licenseRequest.UserAnswerText;
            return result;
        }

      

        public static LicenseRequestStatus[] LicenseRequestStatusesViewedByManager()
        {
            return new[]
            {
                LicenseRequestStatus.New,
                LicenseRequestStatus.SentToUser,
                LicenseRequestStatus.ViewedByUser,
                LicenseRequestStatus.SentToManager,
                LicenseRequestStatus.ViewedByManager,
                LicenseRequestStatus.Closed
            };
        }

        public static LicenseRequestStatus[] LicenseRequestStatusesViewedByPerson()
        {
            return new[]
            {
                LicenseRequestStatus.SentToUser,
                LicenseRequestStatus.ViewedByUser,
                LicenseRequestStatus.SentToManager,
                LicenseRequestStatus.ViewedByManager,
                LicenseRequestStatus.Closed
            };
        }

        public static LicenseRequestPermission GetManagerLicenseRequestPermission(LicenseRequestStatus currentStatus)
        {
            LicenseRequestPermission result;
            switch (currentStatus)
            {
                case LicenseRequestStatus.New:
                    result = LicenseRequestPermission.Update | LicenseRequestPermission.MoveToArchive;
                    break;
                case LicenseRequestStatus.SentToManager:
                    result = LicenseRequestPermission.CreateLicense | LicenseRequestPermission.MoveToArchive;
                    break;
                case LicenseRequestStatus.ViewedByManager:
                    result = LicenseRequestPermission.CreateLicense | LicenseRequestPermission.MoveToArchive;
                    break;
                case LicenseRequestStatus.Closed:
                    result = LicenseRequestPermission.MoveToArchive;
                    break;
                default:
                    result = LicenseRequestPermission.None;
                    break;
            }
            if (LicenseRequestStatusesViewedByManager().Contains(currentStatus))
            {
                result = result | LicenseRequestPermission.View;
            }
            return result;
        }



        public static LicenseRequestPermission GetPersonLicenseRequestPermission(LicenseRequestStatus currentStatus)
        {
            LicenseRequestPermission result;
            switch (currentStatus)
            {
                case LicenseRequestStatus.SentToUser:
                    result = LicenseRequestPermission.CreateAnswer;
                    break;
                case LicenseRequestStatus.ViewedByUser:
                    result = LicenseRequestPermission.CreateAnswer;
                    break;
                default:
                    result = LicenseRequestPermission.None;
                    break;
            }
            if (LicenseRequestStatusesViewedByPerson().Contains(currentStatus))
            {
                result = result | LicenseRequestPermission.View;
            }
            return result;
        }

        public static ManagerLicenseRequestStatus GetManagerLicenseRequestStatus(LicenseRequestStatus currentStatus)
        {
            switch (currentStatus)
            {
                case LicenseRequestStatus.New:
                    return ManagerLicenseRequestStatus.New;
                case LicenseRequestStatus.SentToUser:
                    return ManagerLicenseRequestStatus.AwaitingResponse;
                case LicenseRequestStatus.ViewedByUser:
                    return ManagerLicenseRequestStatus.AwaitingResponse;
                case LicenseRequestStatus.SentToManager:
                    return ManagerLicenseRequestStatus.SentToManager;
                case LicenseRequestStatus.ViewedByManager:
                    return ManagerLicenseRequestStatus.ViewedByManager;
                case LicenseRequestStatus.Closed:
                    return ManagerLicenseRequestStatus.Closed;
                case LicenseRequestStatus.Archived:
                    return ManagerLicenseRequestStatus.Archived;
                default:
                    throw new NotImplementedException();    
            }
        }

        public static PersonalLicenseRequestStatus GetPersonalLicenseRequestStatus(LicenseRequestStatus currentStatus)
        {
            switch (currentStatus)
            {
                case LicenseRequestStatus.SentToUser:
                    return PersonalLicenseRequestStatus.New;
                case LicenseRequestStatus.ViewedByUser:
                    return PersonalLicenseRequestStatus.ViewedByUser;
                case LicenseRequestStatus.SentToManager:
                    return PersonalLicenseRequestStatus.Answered;
                case LicenseRequestStatus.ViewedByManager:
                    return PersonalLicenseRequestStatus.Answered;
                case LicenseRequestStatus.Closed:
                    return PersonalLicenseRequestStatus.Answered;
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetLicenseRequestManagerStatusEn(ManagerLicenseRequestStatus currentStatus)
        {
            switch (currentStatus)
            {
                case ManagerLicenseRequestStatus.New:
                    return "Draft";
                case ManagerLicenseRequestStatus.AwaitingResponse:
                    return "Awaiting response";
                case ManagerLicenseRequestStatus.SentToManager:
                    return "New received";
                case ManagerLicenseRequestStatus.ViewedByManager:
                    return "Received";
                case ManagerLicenseRequestStatus.Closed:
                    return "Closed";
                case ManagerLicenseRequestStatus.Archived:
                    return "Archived";
                default:
                    throw new NotImplementedException();
            }
        }

        public static string GetLicenseRequestUserStatusEn(PersonalLicenseRequestStatus currentStatus)
        {
            switch (currentStatus)
            {
                case PersonalLicenseRequestStatus.New:
                    return "New received";
                case PersonalLicenseRequestStatus.ViewedByUser:
                    return "Received";
                case PersonalLicenseRequestStatus.Answered:
                    return "Answered";
                default:
                    throw new NotImplementedException();
            }
        }

        public static PersonalMachineModel MapToPersonalMachineModel(Machine machine)
        {
            var result = new PersonalMachineModel();
            result.MachineId = machine.UniqueId;
            result.Name = machine.Name;
            result.TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount;
            result.LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount;
            result.UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount;
            result.ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount;
            return result;
        }


        public static PersonalLicenseRequestModel MapToPersonalLicenseRequestModel(LicenseRequest licenseRequest)
        {
            var result = new PersonalLicenseRequestModel();
            result.LicenseRequestId = licenseRequest.UniqueId;
            result.MachineId = licenseRequest.Machine.UniqueId;
            result.MachineName = licenseRequest.Machine.Name;
            result.SoftwareId = licenseRequest.Software.UniqueId;
            result.SoftwareName = licenseRequest.Software.Name;
            result.SoftwarePublisher = licenseRequest.Software.Publisher != null ? licenseRequest.Software.Publisher.Name : string.Empty;
            result.Text = licenseRequest.RequestText;
            result.UserEmail = licenseRequest.UserEmail;
            result.UserId = licenseRequest.UserUserId;
            result.UserName = licenseRequest.User.UserName;
            result.CreatedOn = licenseRequest.LicenseRequestHistories.Min(lrh => lrh.StatusDateTime);
            result.ModifiedOn = licenseRequest.LicenseRequestHistories.Max(lrh => lrh.StatusDateTime);
            result.Status = GetLicenseRequestUserStatusEn(GetPersonalLicenseRequestStatus(licenseRequest.CurrentStatus));
            result.Permission = GetPersonLicenseRequestPermission(licenseRequest.CurrentStatus);
            result.AnswerText = licenseRequest.UserAnswerText;
            result.Documents = licenseRequest.LicenseRequestDocuments.Select(MapToPersonalDocumentModel).ToArray();
            return result;
        }

        private static PersonalDocumentModel MapToPersonalDocumentModel(LicenseRequestDocument document)
        {
            var result = new PersonalDocumentModel();
            result.Id = document.UniqueId;
            result.Name = document.Name;
            return result;
        }

        public static LicenseRequestDocumentModelEx MapToLicenseRequestDocumentModelEx(LicenseRequestDocument document)
        {
            var result = new LicenseRequestDocumentModelEx();
            result.Content = document.Content;
            result.Id = document.UniqueId;
            result.Name = document.Name;
            return result;
        }

        public static PersonalLicenseRequestDocumentModelEx MapToPersonalLicenseRequestDocumentModelEx(LicenseRequestDocument document)
        {
            var result = new PersonalLicenseRequestDocumentModelEx();
            result.Content = document.Content;
            result.Id = document.UniqueId;
            result.Name = document.Name;
            return result;
        }

        public static ObservableModelEx MapToObservableModelEx(Observable observable)
        {
            var result = new ObservableModelEx();
            result.ObservableId = observable.UniqueId;
            result.ProcessName = observable.ProcessName;
            result.SoftwareId = observable.Software.UniqueId;
            result.SoftwareName = observable.Software.Name;
            result.PublisherName = observable.Software.Publisher != null ? observable.Software.Publisher.Name : string.Empty;
            result.CreatedBy = observable.CreatedByUser.UserName;
            //todo AppendedMachines -  получать только количество тех машин права на которые имеет пользователь как менеджер
            result.AppendedMachines = observable.MachineObservedProcesses.Select(mop => mop.Machine).Count();
            return result;
        }

        public static ObservableModel MapToObservableModel(Observable observable)
        {
            var result = new ObservableModel();
            result.ObservableId = observable.UniqueId;
            result.ProcessName = observable.ProcessName;
            result.SoftwareId = observable.Software.UniqueId;
            result.SoftwareName = observable.Software.Name;
            return result;
        }

        public static StructureUnitModel MapToStructureModel(StructureUnit structureUnit)
        {
            return new StructureUnitModel
                       {
                           UniqueId = structureUnit.UniqueId,
                           ParentUniqueId =
                               structureUnit.UnitType != UnitType.Company
                                   ? structureUnit.ParentStructureUnit.UniqueId
                                   : (Guid?) null,
                           Name = structureUnit.Name,
                           ShortName = structureUnit.ShortName
                       };
        }

        public static AccountModel MapToAccountModel(Account account,int installedMachineCount)
        {
            return new AccountModel
            {
                AccountName = account.Name,
                MachineCount = account.MachineCount,
                InstalledMachineCount = installedMachineCount,
                AvailableMachineCount = account.MachineCount - installedMachineCount
            };
        }

        public static UserRoleSimpleModel MapToUserRoleSimpleModel(StructureUnitUserRole user, Guid structureUnitId)
        {
            return new UserRoleSimpleModel
            {
                RoleName = user.Role.Name,
                UserName = user.User.UserName,
                UserId = user.UserUserId,
                RoleId = user.RoleRoleId,
                StructureUnitId = user.StructureUnit.UniqueId,
                StructureUnitName = user.StructureUnit.ShortName,
                IsInherited = user.StructureUnit.UniqueId != structureUnitId
            };
        }

        public static RoleModel MapToRoleModel(CustomRole role)
        {
            return new RoleModel
            {
                RoleName = role.Name,
                RoleId = role.Id
            };
        }

        public static string GetLicensedMachineFilterTypeEn(LicensedMachineFilterType currentStatus)
        {
            switch (currentStatus)
            {
                case LicensedMachineFilterType.None:
                    return "Non licensed";
                case LicensedMachineFilterType.PartialLicensed:
                    return "Partial licensed";
                case LicensedMachineFilterType.Licensed:
                    return "Licensed";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}