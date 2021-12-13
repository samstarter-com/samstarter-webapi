using Newtonsoft.Json;

namespace SWI.SoftStock.ServerApps.WebApplicationModel
{
    public class FailedResponse
    {
        [JsonProperty("success")]
        public bool Success => false;

        [JsonProperty("errors")]
        public string[] Errors { get; set; }
    }
}
