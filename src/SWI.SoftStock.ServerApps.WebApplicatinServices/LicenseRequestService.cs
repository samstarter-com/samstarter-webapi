using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.DataModel2.Identity;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetByStructureUnitId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequest;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.GetNewLicenseRequestCount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SWI.SoftStock.ServerApps.WebApplicationContracts.LicenseRequestService.CreateLicense;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class LicenseRequestService : ILicenseRequestService
    {
        private readonly ILogger<LicenseRequestService> log;
        private readonly CustomUserManager customUserManager;
        private readonly IDbContextFactory<MainDbContext> dbFactory;

        public LicenseRequestService(ILogger<LicenseRequestService> log, CustomUserManager customUserManager, IDbContextFactory<MainDbContext> dbFactory)
        {
            this.log = log;
            this.customUserManager = customUserManager;
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region ILicenseRequestService Members

        public async Task<NewLicenseRequestResponse> GetNewLicenseRequest(NewLicenseRequestRequest request)
        {
            var result = new NewLicenseRequestResponse();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Machine machine = await unitOfWork.MachineRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == request.MachineId);
            if (machine == null)
            {
                result.Status = NewLicenseRequestStatus.MachineNotFound;
                return result;
            }

            if (machine.CurrentUserId == null)
            {
                result.Status = NewLicenseRequestStatus.UserNotFound;
                return result;
            }

            Software software = await unitOfWork.SoftwareRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == request.SoftwareId);
            if (software == null)
            {
                result.Status = NewLicenseRequestStatus.SoftwareNotFound;
                return result;
            }

            User user = customUserManager.Users.SingleOrDefault(l => l.Id == machine.CurrentUserId);
            if (user == null)
            {
                result.Status = NewLicenseRequestStatus.UserNotFound;
                return result;
            }

            IQueryable<MachineSoftware> machineSoftwares =
                unitOfWork.MachineSoftwareRepository.GetAll().Where(
                    ms => ms.MachineId == machine.Id && ms.SoftwareId == software.Id);
            if (!machineSoftwares.Any())
            {
                result.Status = NewLicenseRequestStatus.SoftwareOnMachineNotFound;
                return result;
            }

            result.Model = new NewLicenseRequestModel
            {
                MachineName = machine.Name,
                MachineId = machine.UniqueId,
                SoftwareName = software.Name,
                SoftwarePublisher = software.Publisher != null ? software.Publisher.Name : string.Empty,
                SoftwareId = software.UniqueId,
                UserName = user.UserName,
                UserEmail = user.Email,
                UserId = user.Id,
                Text = GetLicenseRequestText(software.Name, machine.Name)
            };
            result.Status = NewLicenseRequestStatus.Success;
            return result;
        }

        public async Task<Tuple<Guid?, SaveLicenseRequestStatus>> Add(NewLicenseRequestModel model, Guid managerId, bool sending)
        {
            LicenseRequest licenseRequest;
            SaveLicenseRequestStatus status;
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                Machine machine =
                    await unitOfWork.MachineRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == model.MachineId);
                if (machine == null)
                {
                    status = SaveLicenseRequestStatus.MachineNotFound;
                    return null;
                }

                if (machine.CurrentUserId == null)
                {
                    status = SaveLicenseRequestStatus.UserNotFound;
                    return null;
                }

                Software software =
                    await unitOfWork.SoftwareRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == model.SoftwareId);
                if (software == null)
                {
                    status = SaveLicenseRequestStatus.SoftwareNotFound;
                    return null;
                }

                User user = customUserManager.Users.SingleOrDefault(l => l.Id == model.UserId);
                if (user == null)
                {
                    status = SaveLicenseRequestStatus.UserNotFound;
                    return null;
                }

                model.UserEmail = user.Email;
                User manager = customUserManager.Users.SingleOrDefault(l => l.Id == managerId);
                if (manager == null)
                {
                    status = SaveLicenseRequestStatus.ManagerNotFound;
                    return null;
                }

                IQueryable<MachineSoftware> machineSoftwares =
                    unitOfWork.MachineSoftwareRepository.GetAll().Where(
                        ms => ms.MachineId == machine.Id && ms.SoftwareId == software.Id);
                if (!machineSoftwares.Any())
                {
                    status = SaveLicenseRequestStatus.SoftwareOnMachineNotFound;
                    return null;
                }
                licenseRequest = MapperFromViewToModel.MapToLicenseRequest(model,
                    machine.Id,
                    software,
                    sending
                        ? LicenseRequestStatus.SentToUser
                        : LicenseRequestStatus.New,
                    DateTime.UtcNow,
                    managerId);
                unitOfWork.LicenseRequestRepository.Add(licenseRequest);
                unitOfWork.Save();
            }
            status = SaveLicenseRequestStatus.Success;
            return new Tuple<Guid?, SaveLicenseRequestStatus>(licenseRequest.UniqueId, status);
        }

        public async Task<LicenseRequestModel> GetLicenseRequestModelByIdAsync(Guid licenseRequestId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license = await unitOfWork.LicenseRequestRepository.GetAll().SingleAsync(l => l.UniqueId == licenseRequestId);
            return license != null ? MapperFromModelToView.MapToManagerLicenseRequestModel(license) : null;
        }

        public GetByStructureUnitIdResponse GetByStructureUnitId(GetByStructureUnitIdRequest request)
        {
            var response = new GetByStructureUnitIdResponse
            {
                Model = new LicenseRequestCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            LicenseRequestStatus[] availableStatuses = MapperFromModelToView.LicenseRequestStatusesViewedByManager();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Expression<Func<LicenseRequest, bool>> statusTypeWhere = (l => false);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereNew = (l => l.CurrentStatus == LicenseRequestStatus.New);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereAwaitingResponse = (l => l.CurrentStatus == LicenseRequestStatus.ViewedByUser || l.CurrentStatus == LicenseRequestStatus.SentToUser);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereSentToManager = (l => l.CurrentStatus == LicenseRequestStatus.SentToManager);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereViewedByManager = (l => l.CurrentStatus == LicenseRequestStatus.ViewedByManager);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereClosed = (l => l.CurrentStatus == LicenseRequestStatus.Closed);

            var expressions = new List<Expression<Func<LicenseRequest, bool>>>();

            if (request.Status.HasFlag(ManagerLicenseRequestStatus.New))
            {
                expressions.Add(statusTypeWhereNew);
            }

            if (request.Status.HasFlag(ManagerLicenseRequestStatus.AwaitingResponse))
            {
                expressions.Add(statusTypeWhereAwaitingResponse);
            }

            if (request.Status.HasFlag(ManagerLicenseRequestStatus.SentToManager))
            {
                expressions.Add(statusTypeWhereSentToManager);
            }

            if (request.Status.HasFlag(ManagerLicenseRequestStatus.ViewedByManager))
            {
                expressions.Add(statusTypeWhereViewedByManager);
            }

            if (request.Status.HasFlag(ManagerLicenseRequestStatus.Closed))
            {
                expressions.Add(statusTypeWhereClosed);
            }

            if (expressions.Count > 0)
            {
                statusTypeWhere = ExpressionExtension.BuildOr(expressions);
            }

            StructureUnit structureUnit = unitOfWork.StructureUnitRepository.Query(s => s.UniqueId == request.StructureUnitId).Single();
            int suId = structureUnit.Id;
            IQueryable<LicenseRequest> query;

            if (!request.IncludeItemsOfSubUnits)
            {
                query = unitOfWork.LicenseRequestRepository.Query(statusTypeWhere)
                    .Where(l => l.User.StructureUnitRoles.FirstOrDefault(sur => sur.Role.Name == "User")
                        .StructureUnitId == suId)
                    .Where(l => availableStatuses.Contains(l.CurrentStatus));
            }
            else
            {
                IEnumerable<int> structureUnitIds = structureUnit.Descendants(sud => sud.ChildStructureUnits).Select(su => su.Id);

                query = unitOfWork.LicenseRequestRepository.Query(statusTypeWhere).Where(
                    l =>
                        structureUnitIds.Contains(l.User.StructureUnitRoles.FirstOrDefault(sur => sur.Role.Name == "User").StructureUnitId))
                    .Where(l => availableStatuses.Contains(l.CurrentStatus));
            }

            int totalRecords = query.Count();

            var keySelector = GetByStructureUnitIdOrderingSelecetor(request.Ordering.Sort);
            IEnumerable<LicenseRequest> licenseRequests;

            if (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc")
            {
                licenseRequests = query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }
            else
            {
                licenseRequests =
                    query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);
            }

            LicenseRequestModel[] items = licenseRequests.Select(MapperFromModelToView.MapToManagerLicenseRequestModel).ToArray();
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByStructureUnitIdStatus.Success;
            return response;
        }

        public async Task<UpdateLicenseRequestStatus> Update(LicenseRequestModel model, bool sending)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            LicenseRequest licenseRequest =
                await unitOfWork.LicenseRequestRepository.GetAll().SingleOrDefaultAsync(
                    l => l.UniqueId == model.LicenseRequestId);
            bool changed = false;
            if (licenseRequest == null)
            {
                return UpdateLicenseRequestStatus.NotExist;
            }

            if (licenseRequest.CurrentStatus != LicenseRequestStatus.New && sending)
            {
                return UpdateLicenseRequestStatus.WrongStatus;
            }
            if (licenseRequest.CurrentStatus == LicenseRequestStatus.New && sending)
            {
                licenseRequest.CurrentStatus = LicenseRequestStatus.SentToUser;
                licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
                licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
                {
                    Status = licenseRequest.CurrentStatus,
                    StatusDateTime =
                        licenseRequest.CurrentStatusDateTime
                });
                changed = true;
            }

            if (model.Text != licenseRequest.RequestText)
            {
                licenseRequest.RequestText = model.Text;
                changed = true;
            }

            if (changed)
            {
                unitOfWork.Save();
            }

            return UpdateLicenseRequestStatus.Success;
        }

        public async Task<LicenseRequestDocumentModelEx> GetDocumentById(Guid id)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            LicenseRequestDocument doc =
                await unitOfWork.LicenseRequestDocumentRepository.GetAll().SingleAsync(d => d.UniqueId == id);
            return MapperFromModelToView.MapToLicenseRequestDocumentModelEx(doc);
        }

        public async Task<SendLicenseRequestStatus> SendToUser(Guid licenseRequestId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            LicenseRequest licenseRequest =
                await unitOfWork.LicenseRequestRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == licenseRequestId);
            if (licenseRequest == null)
            {
                return SendLicenseRequestStatus.NotExist;
            }
            LicenseRequestPermission permission =
                MapperFromModelToView.GetManagerLicenseRequestPermission(licenseRequest.CurrentStatus);
            if (!permission.HasFlag(LicenseRequestPermission.Update))
            {
                return SendLicenseRequestStatus.WrongStatus;
            }

            licenseRequest.CurrentStatus = LicenseRequestStatus.SentToUser;
            licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
            licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
            {
                Status = licenseRequest.CurrentStatus,
                StatusDateTime =
                    licenseRequest.CurrentStatusDateTime
            });
            unitOfWork.Save();

            return SendLicenseRequestStatus.Success;
        }

        public async Task<CreateLicenseBasedOnLicenseRequestResponse> CreateLicenseAsync(CreateLicenseBasedOnLicenseRequestRequest request)
        {
            var response = new CreateLicenseBasedOnLicenseRequestResponse {LicenseId = null};
            var dbContext = dbFactory.CreateDbContext();
            License license;
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                var licenseRequest =
                    await unitOfWork.LicenseRequestRepository.GetAll().SingleOrDefaultAsync(l => l.UniqueId == request.LicenseRequestId);
                if (licenseRequest == null)
                {
                    response.Status = CreateLicenseBasedOnLicenseRequestStatus.NotExist;
                    return response;
                }
                var permission =
                    MapperFromModelToView.GetManagerLicenseRequestPermission(licenseRequest.CurrentStatus);
                if (!permission.HasFlag(LicenseRequestPermission.CreateLicense))
                {
                    response.Status = CreateLicenseBasedOnLicenseRequestStatus.WrongStatus;
                    return response;
                }
                license = CreateLicense(licenseRequest);

                licenseRequest.CurrentStatus = LicenseRequestStatus.Closed;
                licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
                licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
                {
                    Status = licenseRequest.CurrentStatus,
                    StatusDateTime =
                        licenseRequest.CurrentStatusDateTime
                });

                unitOfWork.LicenseRepository.Add(license);
                await unitOfWork.SaveAsync();
            }
            response.LicenseId = license.UniqueId;
            response.Status = CreateLicenseBasedOnLicenseRequestStatus.Success;
            return response;
        }

        public async Task ReceivedAsync(Guid licenseRequestId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var licenseRequest = await unitOfWork.LicenseRequestRepository.GetAll().SingleAsync(l => l.UniqueId == licenseRequestId);
            if (licenseRequest != null && licenseRequest.CurrentStatus == LicenseRequestStatus.SentToManager)
            {
                licenseRequest.CurrentStatus = LicenseRequestStatus.ViewedByManager;
                licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
                licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
                {
                    Status = licenseRequest.CurrentStatus,
                    StatusDateTime =
                        licenseRequest.CurrentStatusDateTime
                });
            }
            await unitOfWork.SaveAsync();
        }

        public async Task<ArchiveLicenseRequestStatus> Archive(Guid licenseRequestId)
        {
            var dbContext = dbFactory.CreateDbContext();
            using (IUnitOfWork unitOfWork = new UnitOfWork(dbContext))
            {
                LicenseRequest licenseRequest =
                    await unitOfWork.LicenseRequestRepository.GetAll().SingleAsync(l => l.UniqueId == licenseRequestId);
                if (licenseRequest != null)
                {
                    licenseRequest.CurrentStatus = LicenseRequestStatus.Archived;
                    licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
                    licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
                    {
                        Status = licenseRequest.CurrentStatus,
                        StatusDateTime =
                            licenseRequest.CurrentStatusDateTime
                    });
                }
                unitOfWork.Save();
            }
            return ArchiveLicenseRequestStatus.Success;
        }

        public async Task<GetNewLicenseRequestCountResponse> GetNewLicenseRequestCount(GetNewLicenseRequestCountRequest request)
        {
            var response = new GetNewLicenseRequestCountResponse();
            var dbContext = dbFactory.CreateDbContext();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            int totalRecords = await unitOfWork.LicenseRequestRepository.GetAll().CountAsync(l => l.UserUserId1 == request.UserId && l.CurrentStatus == LicenseRequestStatus.SentToManager);
            response.Status = GetNewLicenseRequestCountStatus.Success;
            response.Count = totalRecords;
            return response;
        }

        #endregion

        private Expression<Func<LicenseRequest, object>> GetByStructureUnitIdOrderingSelecetor(string sort)
        {
            Expression<Func<LicenseRequest, object>> keySelector = m => m.User.UserName;
            SortModel[] sortModels = LicenseRequestModel.GetSorting();
            if (!string.IsNullOrEmpty(sort) && sortModels != null && sortModels.Any())
            {
                SortModel sortModel = LicenseRequestModel.GetSortModel(sort);
                if (sortModel == null)
                {
                    return keySelector;
                }
                string orderedPropertyName = sortModel.PropertyName;
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.CreatedOn))
                {
                    keySelector = u => u.LicenseRequestHistories.Min(lrh => lrh.StatusDateTime);
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.ModifiedOn))
                {
                    keySelector = u => u.LicenseRequestHistories.Max(lrh => lrh.StatusDateTime);
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.MachineName))
                {
                    keySelector = u => u.Machine.Name;
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.UserName))
                {
                    keySelector = u => u.User.UserName;
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.UserEmail))
                {
                    keySelector = u => u.UserEmail;
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.SoftwareName))
                {
                    keySelector = u => u.Software.Name;
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.SoftwarePublisher))
                {
                    keySelector = u => (u.Software.Publisher != null ? u.Software.Publisher.Name : string.Empty);
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.Text))
                {
                    keySelector = u => u.RequestText;
                }
                if (orderedPropertyName == Nameof<LicenseRequestModel>.Property(e => e.Status))
                {
                    keySelector = u => u.CurrentStatus;
                }
            }
            return keySelector;
        }

        private License CreateLicense(LicenseRequest licenseRequest)
        {
            var result = new License();
            var name = $"License for software {licenseRequest.Software.Name} on machine {licenseRequest.Machine.Name}";
            if (name.Length > 255)
            {
                name = name.Substring(0, 255);
            }
            result.Name = name;
            result.LicenseTypeId = 9;
            result.BeginDate = DateTime.UtcNow;
            result.ExpirationDate = DateTime.UtcNow.AddYears(5);
            result.Comments = "Based on users response on license request";
            result.Count = 1;
            result.LicenseSoftwares = new[] { new LicenseSoftware { SoftwareId = licenseRequest.Software.Id } };
            result.Documents = licenseRequest.LicenseRequestDocuments.Select(CreateDocument).ToArray();
            result.StructureUnitId =
                licenseRequest.User.StructureUnitRoles.FirstOrDefault(sur => sur.Role.Name == "User").
                    StructureUnitId;
            return result;
        }

        private static Document CreateDocument(LicenseRequestDocument d)
        {
            var result = new Document
            {
                Name = d.Name,
                HcLocation = d.HcLocation,
                Content = d.Content
            };
            return result;
        }

        private string GetLicenseRequestText(string softwareName, string machineName)
        {
            // todo place the text in the template repository (see email template and use RazorEngine)
            return $"Please, attach license documents for software \"{softwareName}\" installed on \"{machineName}\"";
        }
    }
}