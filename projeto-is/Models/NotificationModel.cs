using Newtonsoft.Json;
using System.Xml.Serialization;

namespace SOMIOD.Models
{
    public class NotificationModel
    {
        [JsonProperty("event")]
        public string evt { get; set; }
        [JsonProperty("resource-name")]
        public string resource_name { get; set; }
        public string container { get; set; }
        public string timestamp { get; set; }
        public string content { get; set; }
        [JsonProperty("content-type")]
        public string content_type { get; set; }
    }
}