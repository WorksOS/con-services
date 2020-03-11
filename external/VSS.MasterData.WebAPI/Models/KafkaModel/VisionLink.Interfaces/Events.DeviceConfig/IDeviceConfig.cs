using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.DeviceConfig.Context;

namespace VSS.VisionLink.Interfaces.Events.DeviceConfig
{
    public interface IDeviceConfig
    {
        /// <summary>
        /// Device Uid of the Device, for which settings are done.
        /// </summary>
        Guid DeviceUID { get; set; }
        /// <summary>
        /// Group Information With Parameters and Attributes
        /// </summary>
        ParamGroup Group { get; set; }
        /// <summary>
        /// Date and Time of Setting 
        /// </summary>
        TimestampDetail Timestamp { get; set; }
    }
}
