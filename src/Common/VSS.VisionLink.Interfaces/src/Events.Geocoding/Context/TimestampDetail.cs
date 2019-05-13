using System;

namespace VSS.VisionLink.Interfaces.Events.Geocoding.Context
{
  public class TimestampDetail
	{
		public DateTime EventUtc { get; set; }
		public DateTime? ReceivedUtc { get; set; }
		public string Iso8601EventDeviceTime { get; set; }
  }
}
