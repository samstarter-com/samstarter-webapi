using SWI.SoftStock.ServerApps.WebApplicationContracts.Statuses;

namespace SWI.SoftStock.WebApi.Mapper
{
    internal static class AnswerPersonalLicenseRequestStatusMessage
    {       
            public static string GetErrorMessage(AnswerPersonalLicenseRequestStatus status)
            {
                var result = status switch
                {
                    AnswerPersonalLicenseRequestStatus.NotExist => Resources.AnswerPersonalLicenseRequestStatus
                        .AnswerPersonalLicenseRequestStatus_NotExist,
                    AnswerPersonalLicenseRequestStatus.WrongStatus => Resources.AnswerPersonalLicenseRequestStatus
                        .AnswerPersonalLicenseRequestStatus_WrongStatus,
                    _ => Resources.AnswerPersonalLicenseRequestStatus.AnswerPersonalLicenseRequestStatus_UnknownError
                };
                return result;
            }
        
    }
}
