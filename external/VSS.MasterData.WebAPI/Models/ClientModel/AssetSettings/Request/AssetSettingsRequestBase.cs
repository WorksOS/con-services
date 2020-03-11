using CommonModel.AssetSettings;
using CommonModel.Enum;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class AssetSettingsRequestBase : AssetSettingsBase
    {
        /// <summary>
        /// The start date of the week. 
        /// </summary>
        [JsonIgnore]
        public override DateTime StartDate { get; set; }
        /// <summary>
        /// The end date of the week. 
        /// </summary>
        [JsonIgnore]
        public override DateTime? EndDate { get; set; }
        /// <summary>
        /// The asset unique identifier.
        /// </summary>
        [JsonIgnore]
        public override Guid AssetUID { get; set; }

        [JsonProperty(Required =Required.Always, PropertyName ="assetUIds")]
        public List<string> AssetUIds { get; set; }

        [JsonIgnore]
        public IDictionary<AssetTargetType, double?> TargetValues { get; set; }
    }
}
