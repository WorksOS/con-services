using Newtonsoft.Json;

namespace VSS.TCCFileAccess.Models
{
    public class FileSpace
    {
        [JsonProperty(PropertyName = "filespaceId", Required = Required.Default)]
        public string filespaceId;
        [JsonProperty(PropertyName = "orgDisplayName", Required = Required.Default)]
        public string orgDisplayName;
        [JsonProperty(PropertyName = "orgId", Required = Required.Default)]
        public string orgId;
        [JsonProperty(PropertyName = "orgShortname", Required = Required.Default)]
        public string orgShortname;
        [JsonProperty(PropertyName = "shortname", Required = Required.Default)]
        public string shortname;
        [JsonProperty(PropertyName = "title", Required = Required.Default)]
        public string title;
    }
}