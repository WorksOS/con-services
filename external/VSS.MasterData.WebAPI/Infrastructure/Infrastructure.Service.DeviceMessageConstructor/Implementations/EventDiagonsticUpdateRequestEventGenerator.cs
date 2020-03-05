using System;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.PL;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
    public class EventDiagonsticUpdateRequestEventGenerator : IEventDiagonsticUpdateRequestEventGenerator
    {
        public IPLOutMessageEvent GetEventDiagonsticUpdateRequestEventGenerator(DeviceDetails deviceDetails)
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
                Command = PLQueryCommandEnum.EventDiagnosticQuery
            };
        }
    }
}
