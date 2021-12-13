using System.ComponentModel.DataAnnotations;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class RefreshModel
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}