using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.DeviceConfig;

namespace Infrastructure.Service.DeviceMessagePublisher.Interfaces
{
	public interface IDeviceMessageKafkaPublisher
    {
        bool PublishMessage(string kafkaKey, IEnumerable<object> kafkaObject, string deviceFamilyOrDeviceParam);
        bool PublishDeviceConfiguredMessage(List<DeviceConfig> deviceConfiguredMessages);
		bool PublishMessage(IEnumerable<dynamic> kafkaObjects, string deviceFamily);
	}
}
