
namespace VSS.VisionLink.Interfaces.Events.Geocoding.Context
{
	public class LocationEventDetail
	{
		public AssetDetail Asset { get; set; }
		public DeviceDetail Device { get; set; }
		public OwnerDetail Owner { get; set; }
		public TimestampDetail Timestamp { get; set; }
		public TracingMetadataDetail TracingMetadata { get; set; }

		public double? KilometersAltitude { get; set; }
		public double Latitude { get; set; }
		public double? LocationAge { get; set; }
		public double? LocationUncertaintyMeters { get; set; }
		public bool LocIsValid { get; set; }
		public double Longitude { get; set; }
	}
}
