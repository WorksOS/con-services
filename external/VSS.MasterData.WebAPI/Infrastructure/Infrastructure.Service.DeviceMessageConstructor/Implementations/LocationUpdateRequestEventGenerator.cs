using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using System;
using VSS.VisionLink.Interfaces.Events.Commands.A5N2;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	public class LocationUpdateRequestEventGenerator : ILocationUpdateRequestEventGenerator
    {
        public IOutMessageEvent GetLocationMessageForA5N2(DeviceDetails deviceDetails)
        {
            IOutMessageEvent A5N2LocationRequest = new LocationStatusUpdateRequestedEvent
            {
                Context = new EventContext()
                {
                    AssetUid = deviceDetails.AssetUid.ToString(),
                    DeviceId = deviceDetails.SerialNumber,
                    DeviceType = deviceDetails.DeviceType,
                    DeviceUid = deviceDetails.DeviceUid.ToString(),
                    EventUtc = DateTime.UtcNow,
                    MessageUid = Guid.NewGuid().ToString()
                },
            };
            return A5N2LocationRequest;
        }

        public IMTSOutMessageEvent GetLocationMessageForMTS(DeviceDetails deviceDetails)
        {
            IMTSOutMessageEvent MTSLocationRequest = new PollPositionEvent
            {
                Context = new EventContext()
                {
                    AssetUid = deviceDetails.AssetUid.ToString(),
                    DeviceId = deviceDetails.SerialNumber,
                    DeviceType = deviceDetails.DeviceType,
                    DeviceUid = deviceDetails.DeviceUid.ToString(),
                    EventUtc = DateTime.UtcNow,
                    MessageUid = Guid.NewGuid().ToString()
                }
            };
            return MTSLocationRequest;
        }

        public IPLOutMessageEvent GetLocationMessageForPLDevices(DeviceDetails deviceDetails)
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
                Command = PLQueryCommandEnum.StatusQuery
            };
        }
    }
}
