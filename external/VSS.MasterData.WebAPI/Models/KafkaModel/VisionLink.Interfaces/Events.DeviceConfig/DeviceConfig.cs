using System;
using VSS.VisionLink.Interfaces.Events.DeviceConfig.Context;

namespace VSS.VisionLink.Interfaces.Events.DeviceConfig
{
    public class DeviceConfig : IDeviceConfig
    {
        /// <summary>
        /// Device Uid of the Device, for which settings are done.
        /// </summary>
        public Guid DeviceUID { get; set; }
        /// <summary>
        /// Group Information With Parameters and Attributes
        /// </summary>
        public ParamGroup Group { get; set; }
        /// <summary>
        /// Date and Time of Setting 
        /// </summary>
        public TimestampDetail Timestamp { get; set; }
    }
}