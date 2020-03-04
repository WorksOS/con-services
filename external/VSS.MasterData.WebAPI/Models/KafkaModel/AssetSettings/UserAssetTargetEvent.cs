using CommonModel.Enum;
using Newtonsoft.Json;
using System;

namespace KafkaModel.AssetSettings
{
	public class UserAssetTargetEvent
	{
		public Guid AssetUID { get; set; }
		public DateTime StartDate { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
		public DateTime? EndDate { get; set; }
		public TimestampDetail Timestamp { get; set; }
		public AssetTargetType TargetType { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
		public Guid? UserUID { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public Guid? CustomerUID { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public double? TargetValue { get; set; }
		public bool IsSystemGenerated { get; set; }
	}
}
