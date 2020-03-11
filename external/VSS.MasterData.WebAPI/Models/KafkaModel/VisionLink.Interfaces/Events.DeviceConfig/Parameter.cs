using System.Collections.Generic;

namespace VSS.VisionLink.Interfaces.Events.DeviceConfig
{
    public class Parameter
    {
        /// <summary>
        /// List of Attributes for the parameter
        /// </summary>
        public List<Attributes> Attributes { get; set; }
        /// <summary>
        /// Parameter Name
        /// </summary>
        public string ParameterName { get; set; }
    }
}