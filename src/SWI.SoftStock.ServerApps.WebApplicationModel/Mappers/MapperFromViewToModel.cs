using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using System.Linq;

namespace SWI.SoftStock.ServerApps.WebApplicationServices
{
    using SWI.SoftStock.ServerApps.WebApplicationModel.Common;
    using System;
    using PagingModel = PagingModel;

    public static class MapperFromViewToModel
    {
        public static License MapToLicense(LicenseModelEx model, StructureUnit structureUnit, Software[] softwares)
        {
            var result = new License();
            result.Name = model.Name;
            result.LicenseTypeId = model.LicenseTypeId.Value;
            result.BeginDate = model.BeginDate;
            result.ExpirationDate = model.ExpirationDate;
            result.Comments = model.Comments != null ? model.Comments.Trim() : string.Empty;
            result.Count = model.Count;
            result.LicenseSoftwares = softwares.Select(s => new LicenseSoftware() {SoftwareId = s.Id}).ToArray();
            result.Documents = model.Documents?.Select(MapToDocument).ToArray();
            result.LicenseAlerts = model.Alerts?.Select(MapToAlert).ToArray();
            result.StructureUnitId = structureUnit.Id;
            return result;
        }

        public static Document MapToDocument(DocumentModelEx d)
        {
            var result = new Document();
            result.UniqueId = d.Id;
            result.Name = d.Name;
            result.HcLocation = d.HcLocation;
            result.Content = d.Content;
            return result;
        }

        public static LicenseAlert MapToAlert(AlertModelEx am)
        {
            var result = new LicenseAlert();
            if (am.Id.HasValue)
            {
                result.UniqueId = am.Id.Value;
            }
            result.AlertDate = am.AlertDateTime;
            result.Text = am.AlertText;
            result.Assignees = am.AlertUsersId.Select(Guid.Parse)
                .Select(uId => new LicenseAlertUser { UserUserId = uId }).ToArray();
            return result;
        }

        public static LicenseRequest MapToLicenseRequest(NewLicenseRequestModel model, int machineId, Software software,LicenseRequestStatus status, DateTime statusDateTime, Guid managerId)
        {
            var result = new LicenseRequest();
            result.MachineId = machineId;
            result.Software = software;
            result.UserUserId = model.UserId;
            result.UserEmail = model.UserEmail;
            result.RequestText = model.Text;
            result.CurrentStatus = status;
            result.CurrentStatusDateTime = statusDateTime;
            result.UserUserId1 = managerId;
            return result;
        }

        public static Observable MapToObservable(ObservableModelEx modelEx, int companyId, int softwareId, Guid createdByUserId)
        {
            var result = new Observable();
            result.ProcessName = modelEx.ProcessName;
            result.SoftwareId = softwareId;
            result.CreatedByUserId = createdByUserId;
            result.CompanyId = companyId;
            return result;
        }

        public static Feedback MapToFeedback(FeedbackModel model, string userIp)
        {
            var result = new Feedback();
            result.Title = model.Title;
            result.Comment = model.Comment;
            result.Email = model.Email;
            result.UserIp = userIp;
            return result;
        }

        public static WebApplicationModel.Common.PagingModel MapToPaging(PagingModel model)
        {
            var result = new WebApplicationModel.Common.PagingModel();
            result.PageIndex = model.PageIndex;
            result.PageSize = model.PageSize;
            return result;
        }

        public static OrderModel MapToOrdering(OrderingModel model)
        {
            var result = new SWI.SoftStock.ServerApps.WebApplicationModel.Common.OrderModel();
            result.Order = model.Order;
            result.Sort = model.Sort;
            return result;
        }
    }
}