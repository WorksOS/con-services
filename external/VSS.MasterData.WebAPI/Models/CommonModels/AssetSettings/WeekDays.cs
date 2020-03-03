using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
    public class WeekDays
    {
        [JsonProperty(Required = Required.Always, PropertyName = "sunday")]
        public double Sunday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "monday")]
        public double Monday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "tuesday")]
        public double Tuesday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "wednesday")]
        public double Wednesday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "thursday")]
        public double Thursday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "friday")]
        public double Friday { get; set; }
        [JsonProperty(Required = Required.Always, PropertyName = "saturday")]
        public double Saturday { get; set; }
        //[JsonProperty("assetweeklyconfiguid")]
        [JsonIgnore]
        public Guid AssetWeeklyConfigUID { get; set; }
    }
}
