using System;
using System.Collections.Generic;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
    public class ECMInfoUpdateRequestEventGenerator : IECMInfoUpdateRequestEventGenerator
    {
        public IMTSOutMessageEvent GetDevicePersonalityRequest(DeviceDetails deviceDetails)
        {
            IMTSOutMessageEvent ecmInfoMessage = new SendPersonalityRequestEvent
            {
                Context = new EventContext
                {
                    AssetUid = deviceDetails.AssetUid.ToString(),
                    DeviceId = deviceDetails.SerialNumber,
                    DeviceType = deviceDetails.DeviceType,
                    DeviceUid = deviceDetails.DeviceUid.ToString(),
                    EventUtc = DateTime.UtcNow,
                    MessageUid = Guid.NewGuid().ToString()
                }
            };
            return ecmInfoMessage;
        }

        public IMTSOutMessageEvent GetECMRequestMessageForMTS(DeviceDetails deviceDetails)
        {
            IMTSOutMessageEvent ecmInfoMessage = new SendVehicleBusRequestEvent
            {
                Context = new EventContext
                {
                    AssetUid = deviceDetails.AssetUid.ToString(),
                    DeviceId = deviceDetails.SerialNumber,
                    DeviceType = deviceDetails.DeviceType,
                    DeviceUid = deviceDetails.DeviceUid.ToString(),
                    EventUtc = DateTime.UtcNow,
                    MessageUid = Guid.NewGuid().ToString()
                },
                GatewayMessageTypes = new List<VehicleBusMessageType>() { VehicleBusMessageType.ECMInfo }
            };
            return ecmInfoMessage;
        }
    }
}
