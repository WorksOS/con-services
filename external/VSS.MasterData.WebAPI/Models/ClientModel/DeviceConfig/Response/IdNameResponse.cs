using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response
{
    public class IdNameResponse
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public ulong Id { get; set; }
    }
}
