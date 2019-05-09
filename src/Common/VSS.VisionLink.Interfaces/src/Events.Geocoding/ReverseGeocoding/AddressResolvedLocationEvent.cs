using VSS.VisionLink.Interfaces.Events.Geocoding.Context;

namespace VSS.VisionLink.Interfaces.Events.Geocoding.ReverseGeocoding
{
  public class AddressResolvedLocationEvent
  {
		public LocationEventDetail LocationEvent { get; set; }
		public AddressDetail Address { get; set; }

    public string TimeZone { get; set; }
  }
}
