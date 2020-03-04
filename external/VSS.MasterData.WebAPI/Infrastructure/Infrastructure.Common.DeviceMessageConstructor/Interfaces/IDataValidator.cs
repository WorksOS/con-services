
using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Models;
using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Common.DeviceMessageConstructor.Interfaces
{
    public interface IDataValidator
    {
        bool NullCheck(string modelName, params object[] nullableObjs);
        bool ValueCheck(string modelName, params object[] nullableObjs);
        bool TryParseEventMessage(IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> plOutEvents, ref List<DeviceDetails> plOutDeviceDetails);
        bool TryParseEventMessage(IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> mtsOutEvents, ref List<DeviceDetails> mtsOutDeviceDetails);
        bool TryParseEventMessage(IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> dataOutEvents, ref List<DeviceDetails> dataOutDeviceDetails);

        bool TryParseEventMessage(IEnumerable<Tuple<RuntimeHoursOffset, DeviceDetails>> deviceMessasges, ref List<object> runtimeHoursEvents, ref List<DeviceDetails> dataOutDeviceDetails);
        bool TryParseEventMessage(IEnumerable<Tuple<OdometerOffset, DeviceDetails>> deviceMessasges, ref List<object> odometerEvents, ref List<DeviceDetails> dataOutDeviceDetails);
    }
}
