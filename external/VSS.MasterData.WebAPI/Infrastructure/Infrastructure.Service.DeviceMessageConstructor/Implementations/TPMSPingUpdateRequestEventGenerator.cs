using System;
using System.Collections.Generic;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
    public class TPMSPingUpdateRequestEventGenerator : ITPMSPingUpdateRequestEventGenerator
    {
        public IMTSOutMessageEvent GetTPMSRequestMessageForMTS(DeviceDetails deviceDetails)
        {
            IMTSOutMessageEvent ecmInfoMessage = new SendVehicleBusRequestEvent
            {
                Context = new VSS.VisionLink.Interfaces.Events.Commands.Models.EventContext
                {
                    AssetUid = deviceDetails.AssetUid.ToString(),
                    DeviceId = deviceDetails.SerialNumber,
                    DeviceType = deviceDetails.DeviceType,
                    DeviceUid = deviceDetails.DeviceUid.ToString(),
                    EventUtc = DateTime.UtcNow,
                    MessageUid = Guid.NewGuid().ToString()
                },
                GatewayMessageTypes = new List<VehicleBusMessageType>() { VehicleBusMessageType.TireMonitoring}
            };
            return ecmInfoMessage;
        }
    }
}
