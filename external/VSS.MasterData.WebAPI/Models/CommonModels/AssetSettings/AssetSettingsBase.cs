using ClientModel.Interfaces;
using Newtonsoft.Json;
using System;

namespace CommonModel.AssetSettings
{
	public class AssetSettingsBase : IServiceRequest
    {
        /// <summary>
        /// The start date of the week. 
        /// </summary>
        [JsonProperty("startDate", NullValueHandling = NullValueHandling.Ignore)]
        public virtual DateTime StartDate { get; set; }
        /// <summary>
        /// The end date of the week. 
        /// </summary>
        [JsonProperty("endDate", NullValueHandling = NullValueHandling.Ignore)]
        public virtual DateTime? EndDate { get; set; }
        /// <summary>
        /// The asset unique identifier.
        /// </summary>
        [JsonProperty("assetUid", NullValueHandling = NullValueHandling.Ignore)]
        public virtual Guid AssetUID { get; set; }
        /// <summary>
        /// Customer Uid.
        /// </summary>
        [JsonIgnore]
        public Guid? CustomerUid { get; set; }
        /// <summary>
        /// User Uid.
        /// </summary>
        [JsonIgnore]
        public Guid? UserUid { get; set; }
	}
}
