using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class SendVehicleBusRequestEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }

		public List<VehicleBusMessageType> GatewayMessageTypes { get; set; }
	}
}