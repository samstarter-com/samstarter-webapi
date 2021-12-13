using SWI.SoftStock.ServerApps.DataModel2;

namespace SWI.SoftStock.WebApi.Authentication
{
    public class JwtAuthResult
    {
        public string AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}