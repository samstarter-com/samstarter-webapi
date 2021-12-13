using System.Collections.Generic;

namespace SWI.SoftStock.ServerApps.WebApplicationContracts.SecurityService.ChangePassword
{
    public class ChangePasswordResponse
    {
        public bool Success { get; set; }

        public IEnumerable<string> Errors { get; set; }
    }
}
