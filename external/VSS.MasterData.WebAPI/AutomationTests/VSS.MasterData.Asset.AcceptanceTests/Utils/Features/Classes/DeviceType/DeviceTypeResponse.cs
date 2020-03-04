using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.DeviceType
{
    public class DeviceTypeResponse
    {
        [JsonProperty("deviceTypes")]
        public List<DeviceTypes> DeviceTypes { get; set; }

        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore)]
        public List<Error> errors { get; set; }
    }

    public class DeviceTypes
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
