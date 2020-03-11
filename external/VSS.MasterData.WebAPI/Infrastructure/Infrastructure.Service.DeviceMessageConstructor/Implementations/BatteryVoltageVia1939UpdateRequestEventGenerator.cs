using CommonModel.DeviceSettings;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;
using VSS.VisionLink.Interfaces.Events.Commands.MTS;

namespace Infrastructure.Service.DeviceMessageConstructor.Implementations
{
	public class BatteryVoltageVia1939UpdateRequestEventGenerator : IBatteryVoltageVia1939UpdateRequestEventGenerator
    {
        public IMTSOutMessageEvent GetBatteryVoltageVia1939(DeviceDetails deviceDetails)
        {
            List<J1939ParameterID> periodicParameters = new List<J1939ParameterID>();
            J1939ParameterID batteryVoltageParameter = new J1939ParameterID();
            batteryVoltageParameter.PGN = 65271;
            batteryVoltageParameter.SPN = 168;
            batteryVoltageParameter.SourceAddress = 234;
            periodicParameters.Add(batteryVoltageParameter);
            
            J1939ParameterID kilowattHoursParameter = new J1939ParameterID();
            kilowattHoursParameter.PGN = 65018;
            kilowattHoursParameter.SPN = 2468;
            kilowattHoursParameter.SourceAddress = 234;
            periodicParameters.Add(kilowattHoursParameter);

            return new SendJ1939PublicParametersRequest()
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
                Parameters = periodicParameters
            };

        }
    }
}
