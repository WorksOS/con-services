using System;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// The different varieties of 'passes' that are tracked. Passes are made by the front axle/implement, rear axle/implement, 
    /// vehilce track or vehicle wheel
    /// </summary>
    [Flags]
    public enum PassTypeSet
    {
        /// <summary>
        /// Null value - no pass types set
        /// </summary>
        None = 0,

        /// <summary>
        /// Pass measured at the front axle or machine implement (eg: blade or drum). This is the default value.
        /// </summary>
        Front = 1,

        /// <summary>
        /// Pass measured at the rear axle or machine implement (eg: blade or drum).
        /// </summary>
        Rear = 2,

        /// <summary>
        /// Pass measured at the vehicle track location
        /// </summary>
        Track = 4,

        /// <summary>
        /// Pass measured at the vehicle wheel location
        /// </summary>
        Wheel = 8
    }

}
