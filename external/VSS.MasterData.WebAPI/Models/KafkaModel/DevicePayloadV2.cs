using System;
using System.Collections.Generic;

namespace VSS.MasterData.WebAPI.KafkaModel.Device
{
	public class DevicePayloadV2
	{
		public Guid DeviceUID { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public string DeviceState { get; set; }
		public DateTime? DeregisteredUTC { get; set; }
		public string ModuleType { get; set; }
		public string DataLinkType { get; set; }
		public List<DevicePersonalityPayload> Personalities { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}

	
}
