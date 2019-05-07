using System;

namespace VSS.VisionLink.Interfaces.Events.Commands.A5N2
{
	public class OutMessageStatusEvent 
	{
		public string MessageUid { get; set; }
		public DateTime StatusEventUtc { get; set; }

		public bool Success { get; set; }
	}
}