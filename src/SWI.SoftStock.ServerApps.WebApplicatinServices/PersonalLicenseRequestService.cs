using Microsoft.EntityFrameworkCore;
using SWI.SoftStock.ServerApps.DataAccess2;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetByUserId;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalLicenseRequestService.GetNewLicenseRequestCount;
using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationModel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    public class PersonalLicenseRequestService : IPersonalLicenseRequestService
    {
        private readonly MainDbContextFactory dbFactory;

        public PersonalLicenseRequestService(MainDbContextFactory dbFactory)
        {
            this.dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        #region IPersonalLicenseRequestService Members

        public async Task<GetByUserIdResponse> GetByUserIdAsync(GetByUserIdRequest request)
        {
            var response = new GetByUserIdResponse
            {
                Model = new PersonalLicenseRequestCollection(request.Ordering.Order, request.Ordering.Sort)
            };
            var availableStatuses = MapperFromModelToView.LicenseRequestStatusesViewedByPerson();
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            Expression<Func<LicenseRequest, bool>> statusTypeWhere = (l => false);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereNew = (l => l.CurrentStatus == LicenseRequestStatus.SentToUser);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereViewedByUser = (l => l.CurrentStatus == LicenseRequestStatus.ViewedByUser);
            Expression<Func<LicenseRequest, bool>> statusTypeWhereAnswered = (l => l.CurrentStatus == LicenseRequestStatus.SentToManager || l.CurrentStatus == LicenseRequestStatus.ViewedByManager || l.CurrentStatus == LicenseRequestStatus.Closed);

            var expressions = new List<Expression<Func<LicenseRequest, bool>>>();

            if (request.Status.HasFlag(PersonalLicenseRequestStatus.New))
            {
                expressions.Add(statusTypeWhereNew);
            }

            if (request.Status.HasFlag(PersonalLicenseRequestStatus.ViewedByUser))
            {
                expressions.Add(statusTypeWhereViewedByUser);
            }

            if (request.Status.HasFlag(PersonalLicenseRequestStatus.Answered))
            {
                expressions.Add(statusTypeWhereAnswered);
            }

            if (expressions.Count > 0)
            {
                statusTypeWhere = ExpressionExtension.BuildOr(expressions);
            }

            var query = unitOfWork.LicenseRequestRepository.Query(statusTypeWhere)
                .Where(l => l.UserUserId == request.UserId).Where(l => availableStatuses.Contains(l.CurrentStatus));

            var totalRecords = await query.CountAsync();

            var keySelector = GetByUserIdOrderingSelecetor(request.Ordering.Sort);
            Expression<Func<LicenseRequest, object>> expr = q => q.User;

            var licenseRequests = (string.IsNullOrEmpty(request.Ordering.Order) || request.Ordering.Order.ToLower() != "desc") ?
                query.OrderBy(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize) :
                query.OrderByDescending(keySelector).Skip(request.Paging.PageIndex * request.Paging.PageSize).Take(request.Paging.PageSize);

            var items =
                licenseRequests.Select(MapperFromModelToView.MapToPersonalLicenseRequestModel).ToArray();
            response.Model.Items = items;
            response.Model.TotalRecords = totalRecords;
            response.Status = GetByUserIdStatus.Success;
            return response;
        }

        public async Task<GetNewLicenseRequestCountResponse> GetNewLicenseRequestCount(GetNewLicenseRequestCountRequest request)
        {
            var response = new GetNewLicenseRequestCountResponse();
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            int totalRecords = unitOfWork.LicenseRequestRepository.GetAll().Count(l => l.UserUserId == request.UserId && l.CurrentStatus == LicenseRequestStatus.SentToUser);
            response.Status = GetNewLicenseRequestCountStatus.Success;
            response.Count = totalRecords;
            return response;
        }

        public async Task<PersonalLicenseRequestModel> GetLicenseRequestModelByIdAsync(Guid licenseRequestId)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var license = await
                unitOfWork.LicenseRequestRepository.GetAll().SingleAsync(l => l.UniqueId == licenseRequestId);
            return license != null ? MapperFromModelToView.MapToPersonalLicenseRequestModel(license) : null;
        }

        public async Task ReceivedAsync(Guid licenseRequestId)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var licenseRequest = await unitOfWork.LicenseRequestRepository.GetAll().SingleAsync(l => l.UniqueId == licenseRequestId);
            if (licenseRequest != null && licenseRequest.CurrentStatus == LicenseRequestStatus.SentToUser)
            {
                licenseRequest.CurrentStatus = LicenseRequestStatus.ViewedByUser;
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

        public AnswerPersonalLicenseRequestStatus Answer(PersonalLicenseRequestAnswerModel model)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            var licenseRequest =
                unitOfWork.LicenseRequestRepository.GetAll().SingleOrDefault(
                    l => l.UniqueId == model.LicenseRequestId);
            if (licenseRequest == null)
            {
                return AnswerPersonalLicenseRequestStatus.NotExist;
            }

            if (!MapperFromModelToView.GetPersonLicenseRequestPermission(licenseRequest.CurrentStatus).HasFlag(LicenseRequestPermission.CreateAnswer))
            {
                return AnswerPersonalLicenseRequestStatus.WrongStatus;
            }

            if (model.AnswerText != licenseRequest.UserAnswerText)
            {
                licenseRequest.UserAnswerText = model.AnswerText;
            }

            var oldDocuments = model.Documents.Where(d => !d.IsAddded).ToArray();
            var toRemove = licenseRequest.LicenseRequestDocuments.Where(lrd => oldDocuments.All(od => Guid.Parse(od.Id) != lrd.UniqueId)).ToArray();
            unitOfWork.LicenseRequestDocumentRepository.DeleteRange(toRemove);

            foreach (var document in model.Documents.Where(d => d.IsAddded))
            {
                var uploadedDocument = unitOfWork.UploadedDocumentRepository.GetById(Guid.Parse(document.UploadId));

                licenseRequest.LicenseRequestDocuments.Add(new LicenseRequestDocument
                {
                    Content = uploadedDocument.Content,
                    Name = uploadedDocument.Name,
                    HcLocation = string.Empty
                });
                unitOfWork.UploadedDocumentRepository.Delete(uploadedDocument);
            }

            licenseRequest.CurrentStatus = LicenseRequestStatus.SentToManager;
            licenseRequest.CurrentStatusDateTime = DateTime.UtcNow;
            licenseRequest.LicenseRequestHistories.Add(new LicenseRequestHistory
            {
                Status = licenseRequest.CurrentStatus,
                StatusDateTime =
                    licenseRequest.CurrentStatusDateTime
            });
            unitOfWork.Save();
            return AnswerPersonalLicenseRequestStatus.Success;
        }

        public PersonalLicenseRequestDocumentModelEx GetDocumentById(Guid id)
        {
            var dbContext = dbFactory.Create();
            using IUnitOfWork unitOfWork = new UnitOfWork(dbContext);
            LicenseRequestDocument doc = unitOfWork.LicenseRequestDocumentRepository.GetAll().Single(d => d.UniqueId == id);
            return MapperFromModelToView.MapToPersonalLicenseRequestDocumentModelEx(doc);
        }

        #endregion

        private Expression<Func<LicenseRequest, object>> GetByUserIdOrderingSelecetor(string sort)
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
    }
}