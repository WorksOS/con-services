using CommonModel.DeviceSettings;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	public class UpdateDeviceRequestMessageBuilder : IUpdateDeviceRequestMessageBuilder
    {
        private IUpdateDeviceRequestStatusBuilder _deviceRequestStatusBuilder;
        public UpdateDeviceRequestMessageBuilder(IUpdateDeviceRequestStatusBuilder deviceRequestStatusBuilder)
        {
            _deviceRequestStatusBuilder = deviceRequestStatusBuilder;
        }

        public Guid GetUpdateRequestForDeviceType(DeviceDetails deviceDetails, string deviceTypeFamily, IDictionary<string, string> capabilitiesRecord, ref IList<DeviceACKMessage> deviceAckMessages, ref List<object> kafkaObjects)
        {
            kafkaObjects = new List<Object>(0);
            var messageUID = Guid.NewGuid();
            switch (deviceTypeFamily)
            {
                case "DataOut":
                    kafkaObjects.AddRange(_deviceRequestStatusBuilder.BuildA5N2DeviceStatusUpdateRequestMessage(deviceDetails, capabilitiesRecord));
                    foreach(var item in kafkaObjects.ToList())
                    {
                        deviceAckMessages.Add(new DeviceACKMessage
                        {
                            AssetUID = Guid.Parse((item as IOutMessageEvent).Context.AssetUid).ToString("N"),
                            DeviceUID = Guid.Parse((item as IOutMessageEvent).Context.DeviceUid).ToString("N"),
                            DevicePingLogUID = messageUID.ToString("N"),
                            DevicePingACKMessageUID = Guid.Parse((item as IOutMessageEvent).Context.MessageUid).ToString("N"),
                            RowUpdatedUTC = DateTime.UtcNow,
                            AckStatusID = (int) RequestStatus.Pending
                        });
                    }
                    break;
                case "MTS":
                    kafkaObjects.AddRange(_deviceRequestStatusBuilder.BuildMTSDeviceStatusUpdateRequestMessage(deviceDetails, capabilitiesRecord));
                    foreach (var item in kafkaObjects.ToList())
                    {
                        deviceAckMessages.Add(new DeviceACKMessage
                        {
                            AssetUID = Guid.Parse((item as IMTSOutMessageEvent).Context.AssetUid).ToString("N"),
                            DeviceUID = Guid.Parse((item as IMTSOutMessageEvent).Context.DeviceUid).ToString("N"),
                            DevicePingLogUID = messageUID.ToString("N"),
                            DevicePingACKMessageUID = Guid.Parse((item as IMTSOutMessageEvent).Context.MessageUid).ToString("N"),
                            RowUpdatedUTC = DateTime.UtcNow,
                            AckStatusID = (int)RequestStatus.Pending
                        });
                    }
                    break;
                case "PL":
                    kafkaObjects.AddRange(_deviceRequestStatusBuilder.BuildPLDeviceStatusUpdateRequestMessage(deviceDetails, capabilitiesRecord));
                    foreach (var item in kafkaObjects.ToList())
                    {
                        deviceAckMessages.Add(new DeviceACKMessage
                        {
                            AssetUID = Guid.Parse((item as IPLOutMessageEvent).Context.AssetUid).ToString("N"),
                            DeviceUID =  Guid.Parse((item as IPLOutMessageEvent).Context.DeviceUid).ToString("N"),
                            DevicePingLogUID = messageUID.ToString("N"),
                            DevicePingACKMessageUID = Guid.Parse((item as IPLOutMessageEvent).Context.MessageUid).ToString("N"),
                            RowUpdatedUTC = DateTime.UtcNow,
                            AckStatusID = (int)RequestStatus.Pending
                        });
                    }
                    break;
            }
            return messageUID;
        }
    }
}
