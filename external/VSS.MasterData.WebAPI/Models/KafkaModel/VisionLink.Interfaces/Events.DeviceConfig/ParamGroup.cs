using System.Collections.Generic;

namespace VSS.VisionLink.Interfaces.Events.DeviceConfig
{
    public class ParamGroup
    {
        /// <summary>
        /// Device Group Configuration Name i.e.., Switches, MovingThresholds etc.,
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// List of Parameters for the group
        /// </summary>
        public List<Parameter> Parameters { get; set; }
    }
}