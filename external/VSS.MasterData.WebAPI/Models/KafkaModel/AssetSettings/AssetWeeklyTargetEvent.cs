using CommonModel.Enum;
using Newtonsoft.Json;
using System;

namespace KafkaModel.AssetSettings
{
	public class AssetWeeklyTargetEvent
	{
		public Guid AssetTargetUID { get; set; }
		public Guid AssetUID { get; set; }
		public DateTime StartDate { get; set; }
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
		public DateTime? EndDate { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
		public AssetTargetType TargetType { get; set; }
		public double SundayTargetValue { get; set; }
		public double MondayTargetValue { get; set; }
		public double TuesdayTargetValue { get; set; }
		public double WednesdayTargetValue { get; set; }
		public double ThursdayTargetValue { get; set; }
		public double FridayTargetValue { get; set; }
		public double SaturdayTargetValue { get; set; }
		public bool StatusInd { get; set; }
	}
}
