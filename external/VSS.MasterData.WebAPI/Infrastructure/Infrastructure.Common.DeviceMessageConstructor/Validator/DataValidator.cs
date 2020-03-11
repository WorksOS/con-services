using CommonModel.DeviceSettings;
using Infrastructure.Common.DeviceMessageConstructor.Interfaces;
using Infrastructure.Common.DeviceMessageConstructor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;

namespace Infrastructure.Common.DeviceMessageConstructor.Validator
{
	public class DataValidator : IDataValidator
  {
    public bool NullCheck(string modelName, params object[] nullableObjs)
    {
      foreach (var nullableObj in nullableObjs)
      {
        if (nullableObj == null)
          throw new ArgumentNullException("Null being set for Type in " + modelName);
      }
      return true;
    }

    public bool ValueCheck(string modelName, params object[] nullableObjs)
    {
      if (nullableObjs.Any(obj => obj != null))
        return true;
      throw new ArgumentNullException("All values are null in " + modelName);
    }

     public bool TryParseEventMessage(IEnumerable<Tuple<IPLOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> plOutEvents, ref List<DeviceDetails> plOutDeviceDetails)
     {
        if(deviceMessasges == null) return false;
        var hasDeviceMessage = false;
        foreach (var deviceMessasge in deviceMessasges)
        {
           hasDeviceMessage = true;
           var newGuid = Guid.NewGuid();
           deviceMessasge.Item1.Context.MessageUid = newGuid.ToString();
           deviceMessasge.Item2.MessageUid = newGuid;
           plOutEvents.Add(deviceMessasge.Item1);
           plOutDeviceDetails.Add(deviceMessasge.Item2);
        }
        return hasDeviceMessage;
     }

     public bool TryParseEventMessage(IEnumerable<Tuple<IMTSOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> mtsOutEvents, ref List<DeviceDetails> mtsOutDeviceDetails)
     {
        if (deviceMessasges == null) return false;
        var hasDeviceMessage = false;
        foreach (var deviceMessasge in deviceMessasges)
        {
           hasDeviceMessage = true;
           var newGuid = Guid.NewGuid();
           deviceMessasge.Item1.Context.MessageUid = newGuid.ToString();
           deviceMessasge.Item2.MessageUid = newGuid;
           mtsOutEvents.Add(deviceMessasge.Item1);
           mtsOutDeviceDetails.Add(deviceMessasge.Item2);
        }
        return hasDeviceMessage;
     }

     public bool TryParseEventMessage(IEnumerable<Tuple<IOutMessageEvent, DeviceDetails>> deviceMessasges, ref List<object> dataOutEvents, ref List<DeviceDetails> dataOutDeviceDetails)
     {
        if (deviceMessasges == null) return false;
        var hasDeviceMessage = false;
        foreach (var deviceMessasge in deviceMessasges)
        {
           hasDeviceMessage = true;
           var newGuid = Guid.NewGuid();
           deviceMessasge.Item1.Context.MessageUid = newGuid.ToString();
           deviceMessasge.Item2.MessageUid = newGuid;
           dataOutEvents.Add(deviceMessasge.Item1);
           dataOutDeviceDetails.Add(deviceMessasge.Item2);
        }
        return hasDeviceMessage;
     }

        public bool TryParseEventMessage(IEnumerable<Tuple<RuntimeHoursOffset, DeviceDetails>> deviceMessasges, ref List<object> runtimeHoursEvents, ref List<DeviceDetails> dataOutDeviceDetails)
        {
            if (deviceMessasges == null) return false;
            var hasDeviceMessage = false;
            foreach (var deviceMessasge in deviceMessasges)
            {
                hasDeviceMessage = true;
                var newGuid = Guid.NewGuid();               
                deviceMessasge.Item2.MessageUid = newGuid;
                runtimeHoursEvents.Add(deviceMessasge.Item1);
                dataOutDeviceDetails.Add(deviceMessasge.Item2);
            }
            return hasDeviceMessage;
        }

        public bool TryParseEventMessage(IEnumerable<Tuple<OdometerOffset, DeviceDetails>> deviceMessasges, ref List<object> odometerEvents, ref List<DeviceDetails> dataOutDeviceDetails)
        {
            if (deviceMessasges == null) return false;
            var hasDeviceMessage = false;
            foreach (var deviceMessasge in deviceMessasges)
            {
                hasDeviceMessage = true;
                var newGuid = Guid.NewGuid();
                deviceMessasge.Item2.MessageUid = newGuid;
                odometerEvents.Add(deviceMessasge.Item1);
                dataOutDeviceDetails.Add(deviceMessasge.Item2);
            }
            return hasDeviceMessage;
        }
    }
}
