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
            var result = new T
            {
                SoftwareId = software.UniqueId,
                Name = software.Name,
                PublisherName = software.Publisher != null ? software.Publisher.Name : string.Empty,
                Version = software.Version,
                SystemComponent = software.SystemComponent,
                WindowsInstaller = software.WindowsInstaller,
                ReleaseType = software.ReleaseType
            };
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
            var result = new DocumentModel
            {
                Id = document.UniqueId,
                Name = document.Name,
                HcLocation = document.HcLocation
            };
            return result;
        }

        public static LicenseRequestDocumentModel MapToLicenseRequestDocumentModel(LicenseRequestDocument document)
        {
            var result = new LicenseRequestDocumentModel
            {
                Id = document.UniqueId,
                Name = document.Name,
                HcLocation = document.HcLocation
            };
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
            var result = new AlertModel
            {
                AlertDateTime = alert.AlertDate,
                AlertText = alert.Text,
                AlertUsers = alert.Assignees.Select(a => MapToUserModel(a.User, a.User.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit)).ToArray()
            };
            return result;
        }

        public static AlertModelEx MapToLicenseAlertModelEx(LicenseAlert alert)
        {
            var result = new AlertModelEx
            {
                Id = alert.UniqueId,
                AlertDateTime = alert.AlertDate,
                AlertText = alert.Text,
                AlertUsers = alert.Assignees.Select(a => MapToUserModel(a.User, a.User.StructureUnitRoles.Single(sur => sur.Role.Name == "User").StructureUnit)).ToArray()
            };
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
            var result = new T
            {
                LicenseId = license.UniqueId,
                Name = license.Name,
                LicenseTypeName = license.LicenseType.Name,
                LinkedSoftwares = license.LicenseSoftwares.Select(ls => MapToSoftwareModel<SoftwareModel>(ls.Software)).ToArray(),
                BeginDate = license.BeginDate,
                ExpirationDate = license.ExpirationDate,
                Count = license.Count,
                AvailableCount = license.Count -
                                    license.LicenseSoftwares.SelectMany(
                                        ls => ls.LicenseMachineSoftwares.Where(lms => !lms.IsDeleted)).Select(
                                            lms => lms.MachineSoftware.MachineId).Distinct().Count(),
                Comments = license.Comments,
                Documents = license.Documents.Select(MapToDocumentModel).ToArray(),
                Alerts = license.LicenseAlerts.Select(MapToLicenseAlertModel).ToArray(),
                StructureUnitName = license.StructureUnit.ShortName,
                StructureUnitId = license.StructureUnit.UniqueId
            };
            return result;
        }

        public static DropDownItemModel MapToLicenseTypeModel(LicenseType licenseType)
        {
            var result = new DropDownItemModel
            {
                Id = licenseType.Id,
                Name = licenseType.Name
            };
            return result;
        }

        public static LicenseModelEx MapToLicenseModelEx(License license)
        {
            var result = new LicenseModelEx
            {
                LicenseId = license.UniqueId,
                Name = license.Name,
                LicenseTypeId = license.LicenseTypeId,
                BeginDate = license.BeginDate,
                ExpirationDate = license.ExpirationDate,
                Comments = license.Comments,
                Count = license.Count,
                LinkedSoftwares = license.LicenseSoftwares.Select(ls => MapToSoftwareModel<SoftwareModel>(ls.Software)).ToArray(),
                Documents = license.Documents.Select(d => MapToDocumentModelEx(d, false)).ToArray(),
                Alerts = license.LicenseAlerts.Select(MapToLicenseAlertModelEx).ToArray(),
                StructureUnitId = license.StructureUnit.UniqueId
            };
            return result;
        }

        public static ShortLicenseModel MapToShortLicenseModel(License license)
        {
            var result = new ShortLicenseModel
            {
                LicenseId = license.UniqueId,
                Name = license.Name
            };
            return result;
        }
        
        public static T MapToMachineModel<T>(Machine machine) where T : class, IMachineModel, new()
        {
            var result = new T
            {
                UserId = machine.CurrentUserId,
                MachineId = machine.UniqueId,
                OperationSystemName = machine.MachineOperationSystem?.OperationSystem?.Name,
                LinkedUserName = machine.CurrentUser?.UserName,
                DomainUserDomainName = machine.CurrentDomainUser?.DomainName,
                DomainUserName = machine.CurrentDomainUser?.Name,
                Name = machine.Name,
                Enabled = !machine.IsDisabled,
                CreatedOn = machine.CreatedOn,
                LastActivity = machine.LastActivityDateTime,
                StructureUnitId = machine.CurrentLinkedStructureUnit?.UniqueId,
                StructureUnitName = machine.CurrentLinkedStructureUnit != null ? machine.CurrentLinkedStructureUnit.ShortName : string.Empty,
                TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount,
                LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount,
                UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount,
                ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount
            };
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
            var result = new MachineModelEx
            {
                UserId = machine.CurrentUser?.Id,
                UserName = machine.CurrentUser != null ? machine.CurrentUser.UserName : string.Empty,
                MachineId = machine.UniqueId,
                DomainUserDomainName = machine.CurrentDomainUser?.DomainName,
                DomainUserName = machine.CurrentDomainUser != null ? machine.CurrentDomainUser.Name : null,
                MemoryTotalCapacity = machine.MemoryTotalCapacity,
                MonitorCount = machine.MonitorCount,
                Name = machine.Name,
                StructureUnitId = machine.CurrentLinkedStructureUnit != null ? (Guid?)machine.CurrentLinkedStructureUnit.UniqueId : null,
                StructureUnitName = machine.CurrentLinkedStructureUnit != null ? machine.CurrentLinkedStructureUnit.ShortName : string.Empty,
                TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount,
                LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount,
                UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount,
                ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount,
                CreatedOn = machine.CreatedOn,
                ModifiedOn = machine.ModifiedOn,
                LastActivity = machine.LastActivityDateTime,
                NetworkAdapters = machine.NetworkAdapters.Select(MapToNetworkAdapterModel),
                ObservableProcesses = machine.MachineObservedProcesses.Select(mop => mop.Observable).Select(MapToObservableModel),
                Processor = machine.Processor != null ? MapToProcessorModel(machine.Processor) : null,
                OperationSystem = machine.MachineOperationSystem != null
                ? MapToOperationSystemModel(machine.MachineOperationSystem)
                : null,
                Enabled = !machine.IsDisabled
            };
            return result;
        }

        public static NetworkAdapterModel MapToNetworkAdapterModel(NetworkAdapter networkAdapter)
        {
            var result = new NetworkAdapterModel
            {
                MacAdress = networkAdapter.MacAdress,
                Caption = networkAdapter.Caption
            };
            return result;
        }

        public static ProcessorModel MapToProcessorModel(Processor processor)
        {
            var result = new ProcessorModel
            {
                ProcessorId = processor.ProcessorId,
                DeviceId = processor.DeviceID,
                SocketDesignation = processor.SocketDesignation,
                Is64BitProcess = processor.Is64BitProcess,
                ManufacturerName = processor.Manufacturer.Name
            };
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
            var result = new LicenseRequestModel
            {
                LicenseRequestId = licenseRequest.UniqueId,
                MachineId = licenseRequest.Machine.UniqueId,
                MachineName = licenseRequest.Machine.Name,
                SoftwareId = licenseRequest.Software.UniqueId,
                SoftwareName = licenseRequest.Software.Name,
                SoftwarePublisher = licenseRequest.Software.Publisher != null ? licenseRequest.Software.Publisher.Name : string.Empty,
                Text = licenseRequest.RequestText,
                UserEmail = licenseRequest.UserEmail,
                UserId = licenseRequest.UserUserId,
                UserName = licenseRequest.User.UserName,
                CreatedOn = licenseRequest.LicenseRequestHistories.Min(lrh => lrh.StatusDateTime),
                ModifiedOn = licenseRequest.LicenseRequestHistories.Max(lrh => lrh.StatusDateTime),
                Status = GetLicenseRequestManagerStatusEn(GetManagerLicenseRequestStatus(licenseRequest.CurrentStatus)),
                Permission = GetManagerLicenseRequestPermission(licenseRequest.CurrentStatus),
                Documents = licenseRequest.LicenseRequestDocuments.Select(MapToLicenseRequestDocumentModel).ToArray(),
                UserAnswerText = licenseRequest.UserAnswerText
            };
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
            var result = currentStatus switch
            {
                LicenseRequestStatus.New => LicenseRequestPermission.Update | LicenseRequestPermission.MoveToArchive,
                LicenseRequestStatus.SentToManager => LicenseRequestPermission.CreateLicense | LicenseRequestPermission.MoveToArchive,
                LicenseRequestStatus.ViewedByManager => LicenseRequestPermission.CreateLicense | LicenseRequestPermission.MoveToArchive,
                LicenseRequestStatus.Closed => LicenseRequestPermission.MoveToArchive,
                _ => LicenseRequestPermission.None,
            };
            if (LicenseRequestStatusesViewedByManager().Contains(currentStatus))
            {
                result = result | LicenseRequestPermission.View;
            }
            return result;
        }



        public static LicenseRequestPermission GetPersonLicenseRequestPermission(LicenseRequestStatus currentStatus)
        {
            var result = currentStatus switch
            {
                LicenseRequestStatus.SentToUser => LicenseRequestPermission.CreateAnswer,
                LicenseRequestStatus.ViewedByUser => LicenseRequestPermission.CreateAnswer,
                _ => LicenseRequestPermission.None,
            };
            if (LicenseRequestStatusesViewedByPerson().Contains(currentStatus))
            {
                result = result | LicenseRequestPermission.View;
            }
            return result;
        }

        public static ManagerLicenseRequestStatus GetManagerLicenseRequestStatus(LicenseRequestStatus currentStatus)
        {
            return currentStatus switch
            {
                LicenseRequestStatus.New => ManagerLicenseRequestStatus.New,
                LicenseRequestStatus.SentToUser => ManagerLicenseRequestStatus.AwaitingResponse,
                LicenseRequestStatus.ViewedByUser => ManagerLicenseRequestStatus.AwaitingResponse,
                LicenseRequestStatus.SentToManager => ManagerLicenseRequestStatus.SentToManager,
                LicenseRequestStatus.ViewedByManager => ManagerLicenseRequestStatus.ViewedByManager,
                LicenseRequestStatus.Closed => ManagerLicenseRequestStatus.Closed,
                LicenseRequestStatus.Archived => ManagerLicenseRequestStatus.Archived,
                _ => throw new NotImplementedException(),
            };
        }

        public static PersonalLicenseRequestStatus GetPersonalLicenseRequestStatus(LicenseRequestStatus currentStatus)
        {
            return currentStatus switch
            {
                LicenseRequestStatus.SentToUser => PersonalLicenseRequestStatus.New,
                LicenseRequestStatus.ViewedByUser => PersonalLicenseRequestStatus.ViewedByUser,
                LicenseRequestStatus.SentToManager => PersonalLicenseRequestStatus.Answered,
                LicenseRequestStatus.ViewedByManager => PersonalLicenseRequestStatus.Answered,
                LicenseRequestStatus.Closed => PersonalLicenseRequestStatus.Answered,
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetLicenseRequestManagerStatusEn(ManagerLicenseRequestStatus currentStatus)
        {
            return currentStatus switch
            {
                ManagerLicenseRequestStatus.New => "Draft",
                ManagerLicenseRequestStatus.AwaitingResponse => "Awaiting response",
                ManagerLicenseRequestStatus.SentToManager => "New received",
                ManagerLicenseRequestStatus.ViewedByManager => "Received",
                ManagerLicenseRequestStatus.Closed => "Closed",
                ManagerLicenseRequestStatus.Archived => "Archived",
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetLicenseRequestUserStatusEn(PersonalLicenseRequestStatus currentStatus)
        {
            return currentStatus switch
            {
                PersonalLicenseRequestStatus.New => "New received",
                PersonalLicenseRequestStatus.ViewedByUser => "Received",
                PersonalLicenseRequestStatus.Answered => "Answered",
                _ => throw new NotImplementedException(),
            };
        }

        public static PersonalMachineModel MapToPersonalMachineModel(Machine machine)
        {
            var result = new PersonalMachineModel
            {
                MachineId = machine.UniqueId,
                Name = machine.Name,
                TotalSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresTotalCount,
                LicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsActiveCount,
                UnLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresUnlicensedCount,
                ExpiredLicensedSoftwareCount = machine.MachineSoftwaresReadOnly.SoftwaresIsExpiredCount
            };
            return result;
        }


        public static PersonalLicenseRequestModel MapToPersonalLicenseRequestModel(LicenseRequest licenseRequest)
        {
            var result = new PersonalLicenseRequestModel
            {
                LicenseRequestId = licenseRequest.UniqueId,
                MachineId = licenseRequest.Machine.UniqueId,
                MachineName = licenseRequest.Machine.Name,
                SoftwareId = licenseRequest.Software.UniqueId,
                SoftwareName = licenseRequest.Software.Name,
                SoftwarePublisher = licenseRequest.Software.Publisher != null ? licenseRequest.Software.Publisher.Name : string.Empty,
                Text = licenseRequest.RequestText,
                UserEmail = licenseRequest.UserEmail,
                UserId = licenseRequest.UserUserId,
                UserName = licenseRequest.User.UserName,
                CreatedOn = licenseRequest.LicenseRequestHistories.Min(lrh => lrh.StatusDateTime),
                ModifiedOn = licenseRequest.LicenseRequestHistories.Max(lrh => lrh.StatusDateTime),
                Status = GetLicenseRequestUserStatusEn(GetPersonalLicenseRequestStatus(licenseRequest.CurrentStatus)),
                Permission = GetPersonLicenseRequestPermission(licenseRequest.CurrentStatus),
                AnswerText = licenseRequest.UserAnswerText,
                Documents = licenseRequest.LicenseRequestDocuments.Select(MapToPersonalDocumentModel).ToArray()
            };
            return result;
        }

        private static PersonalDocumentModel MapToPersonalDocumentModel(LicenseRequestDocument document)
        {
            var result = new PersonalDocumentModel
            {
                Id = document.UniqueId,
                Name = document.Name
            };
            return result;
        }

        public static LicenseRequestDocumentModelEx MapToLicenseRequestDocumentModelEx(LicenseRequestDocument document)
        {
            var result = new LicenseRequestDocumentModelEx
            {
                Content = document.Content,
                Id = document.UniqueId,
                Name = document.Name
            };
            return result;
        }

        public static PersonalLicenseRequestDocumentModelEx MapToPersonalLicenseRequestDocumentModelEx(LicenseRequestDocument document)
        {
            var result = new PersonalLicenseRequestDocumentModelEx
            {
                Content = document.Content,
                Id = document.UniqueId,
                Name = document.Name
            };
            return result;
        }

        public static ObservableModelEx MapToObservableModelEx(Observable observable)
        {
            var result = new ObservableModelEx
            {
                ObservableId = observable.UniqueId,
                ProcessName = observable.ProcessName,
                SoftwareId = observable.Software.UniqueId,
                SoftwareName = observable.Software.Name,
                PublisherName = observable.Software.Publisher != null ? observable.Software.Publisher.Name : string.Empty,
                CreatedBy = observable.CreatedByUser.UserName,
                //todo AppendedMachines -  get only the number of those machines that the user has rights to as a manage
                AppendedMachines = observable.MachineObservedProcesses.Select(mop => mop.Machine).Count()
            };
            return result;
        }

        public static ObservableModel MapToObservableModel(Observable observable)
        {
            var result = new ObservableModel
            {
                ObservableId = observable.UniqueId,
                ProcessName = observable.ProcessName,
                SoftwareId = observable.Software.UniqueId,
                SoftwareName = observable.Software.Name
            };
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
            return currentStatus switch
            {
                LicensedMachineFilterType.None => "Non licensed",
                LicensedMachineFilterType.PartialLicensed => "Partial licensed",
                LicensedMachineFilterType.Licensed => "Licensed",
                _ => throw new NotImplementedException(),
            };
        }
    }
}