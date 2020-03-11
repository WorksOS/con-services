using System;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public class EventContext
	{
		public DateTime EventUtc { get; set; }
		public string DeviceType { get; set; }
		public string DeviceId { get; set; }

		public string MessageUid { get; set; }
		public string DeviceUid { get; set; }
		public string AssetUid { get; set; }
	}
}