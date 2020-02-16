using System;

namespace VSS.Nighthawk.MasterDataSync.Models
{
	public enum LocationDisplayTypeEnum
	{
		None = -1,
		Site = 0,
		Address = 1,
		LatLong = 2
	}

	public enum AssetLabelPreferenceTypeEnum
	{
		None = 0,
		AssetId = 1,
		SerialNumber = 2,
		Both = 3
	}

	public class UserPreferenceEvent
	{

		public string Timezone { get; set; }
		public string Language { get; set; }
		public string Units { get; set; } //US Standard, metric
		public string AssetLabelDisplay { get; set; }
		public string MeterLabelDisplay { get; set; }
		public string LocationDisplay { get; set; }
		public string TemperatureUnit { get; set; }
		public string PressureUnit { get; set; } // PSI, kPa, BAR

		public string DateFormat { get; set; }
		public string TimeFormat { get; set; }
		public string CurrencySymbol { get; set; }
		public string ThousandsSeparator { get; set; }
		public string DecimalSeparator { get; set; }
		
		public string DecimalPrecision { get; set; }
		public string MapProvider { get; set; }

	}

	public class UserPreferenceWrapperEvent
	{
		public string PreferenceKeyName = "global";
		public string PreferenceJson { get; set; }
		public DateTime ActionUtc { get; set; }
	}
}
