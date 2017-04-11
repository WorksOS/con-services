using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    ///  The direction the machine is moving in 
    /// </summary>
    public enum MachineDirection
    {
        /// <summary>
        /// Machine is moving in machien defined forward direction
        /// </summary>
        Forward,

        /// <summary>
        /// Machine is moving in machien defined reveres direction
        /// </summary>
        Reverse,

        /// <summary>
        /// Machine direction is null or unknown
        /// </summary>
        Unknown
    }
}
