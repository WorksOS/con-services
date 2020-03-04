using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	public class FuelUpdateRequestEventGenerator : IFuelUpdateRequestEventGenerator
    {
        public IMTSOutMessageEvent GetFuelMessageForMTS(DeviceDetails deviceDetails, FuelRequestType fuelRequest)
        {
            IMTSOutMessageEvent fuelEventMessage;
            if (fuelRequest == FuelRequestType.GatewayRequest)
                fuelEventMessage = new SendGatewayRequestEvent()
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
                    GatewayMessageTypes = new List<GatewayMessageType>() { GatewayMessageType.FuelEngine }
                };
            else
                fuelEventMessage = new SendVehicleBusRequestEvent
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
                    GatewayMessageTypes = new List<VehicleBusMessageType>() { VehicleBusMessageType.FuelEngine }
                };
            return fuelEventMessage;
        }

        public IPLOutMessageEvent GetFuelMessageForPLOut(DeviceDetails deviceDetails)
        {
            return new SendQueryCommandEvent
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
                Command = PLQueryCommandEnum.FuelLevelQuery
            };
        }
    }
}
