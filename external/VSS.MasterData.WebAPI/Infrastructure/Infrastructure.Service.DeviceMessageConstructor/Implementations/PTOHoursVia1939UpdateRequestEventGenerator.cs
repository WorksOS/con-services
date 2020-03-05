using System;
using System.Collections.Generic;
using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
    public class PTOHoursVia1939UpdateRequestEventGenerator : IPTOHoursVia1939UpdateRequestEventGenerator
    {
        public IMTSOutMessageEvent GetPTOHoursviaJ1939(DeviceDetails deviceDetails)
        {
            List<J1939ParameterID> periodicParameters = new List<J1939ParameterID>();
            J1939ParameterID engineTotalPTO = new J1939ParameterID();
            engineTotalPTO.PGN = 65255;
            engineTotalPTO.SPN = 248;
            engineTotalPTO.SourceAddress = 0;
            periodicParameters.Add(engineTotalPTO);

            J1939ParameterID transmissionTotalPTO = new J1939ParameterID();
            transmissionTotalPTO.PGN = 65255;
            transmissionTotalPTO.SPN = 248;
            transmissionTotalPTO.SourceAddress = 3;
            periodicParameters.Add(transmissionTotalPTO);

            return new SendJ1939PublicParametersRequest
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
                Parameters = periodicParameters
            };
        }
    }
}
