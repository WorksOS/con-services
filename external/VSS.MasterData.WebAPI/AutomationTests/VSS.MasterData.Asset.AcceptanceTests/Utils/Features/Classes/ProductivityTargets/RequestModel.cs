using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.ProductivityTargets
{
    public class ProductivityTargetsRequestModel
    {
        [JsonProperty(PropertyName = "Targets")]
        public List<Targets> assettargets { get; set; }
    }

    public class Targetcycles
    {
        [JsonProperty(PropertyName = "sunday")]
        public string Sunday { get; set; }
        [JsonProperty(PropertyName = "monday")]
        public string Monday { get; set; }
        [JsonProperty(PropertyName = "tuesday")]
        public string Tuesday { get; set; }
        [JsonProperty(PropertyName = "wednesday")]
        public string Wednesday { get; set; }
        [JsonProperty(PropertyName = "thursday")]
        public string Thursday { get; set; }
        [JsonProperty(PropertyName = "friday")]
        public string Friday { get; set; }
        [JsonProperty(PropertyName = "saturday")]
        public string Saturday { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssetWeeklyConfigUID { get; set; }
    }

    public class Targetvolumes
    {
        [JsonProperty(PropertyName = "sunday")]
        public string Sunday { get; set; }
        [JsonProperty(PropertyName = "monday")]
        public string Monday { get; set; }
        [JsonProperty(PropertyName = "tuesday")]
        public string Tuesday { get; set; }
        [JsonProperty(PropertyName = "wednesday")]
        public string Wednesday { get; set; }
        [JsonProperty(PropertyName = "thursday")]
        public string Thursday { get; set; }
        [JsonProperty(PropertyName = "friday")]
        public string Friday { get; set; }
        [JsonProperty(PropertyName = "saturday")]
        public string Saturday { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AssetWeeklyConfigUID { get; set; }
    }


    public class Targetpayload
    {
    [JsonProperty(PropertyName = "sunday")]
    public string Sunday { get; set; }
    [JsonProperty(PropertyName = "monday")]
    public string Monday { get; set; }
    [JsonProperty(PropertyName = "tuesday")]
    public string Tuesday { get; set; }
    [JsonProperty(PropertyName = "wednesday")]
    public string Wednesday { get; set; }
    [JsonProperty(PropertyName = "thursday")]
    public string Thursday { get; set; }
    [JsonProperty(PropertyName = "friday")]
    public string Friday { get; set; }
    [JsonProperty(PropertyName = "saturday")]
    public string Saturday { get; set; }
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string AssetWeeklyConfigUID { get; set; }
}
    

    public class Targets
    {
        public Targetcycles targetcycles { get; set; }
        public Targetvolumes targetvolumes { get; set; }
        public Targetpayload targetpayload { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public Guid assetuid { get; set; }
    }



}

