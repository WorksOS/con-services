using CommonModel.Enum;
using Newtonsoft.Json;
using System;

namespace KafkaModel.AssetSettings
{
	public class AssetTargetEvent
	{
		public Guid AssetTargetUID { get; set; }
		public Guid AssetUID { get; set; }
		public DateTime StartDate { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public DateTime? EndDate { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
		public AssetTargetType TargetType { get; set; }
		public double TargetValue { get; set; }
		public bool StatusInd { get; set; }
	}
}
