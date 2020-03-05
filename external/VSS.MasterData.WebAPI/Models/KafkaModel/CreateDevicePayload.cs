using System;

namespace VSS.MasterData.WebAPI.KafkaModel.Device
{
	public class CreateDevicePayload : DevicePayload
	{
	}

	public class UpdateDevicePayload : DevicePayload
	{
		public Guid? OwningCustomerUID { get; set; }
	}

	public enum DeviceStateEnum
	{
		None = 0,
		Installed = 1,
		Provisioned = 2,
		Subscribed = 3,
		DeregisteredTechnician = 4,
		DeregisteredStore = 5

	}
	
}
