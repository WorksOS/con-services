using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class SendGatewayRequestEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public List<GatewayMessageType> GatewayMessageTypes { get; set; }
	}
}