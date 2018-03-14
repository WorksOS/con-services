using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.SubGridTrees
{
    /// <summary>
    /// The options used to control the initial state of a BitMask when it is created
    /// </summary>
    public enum SubGridBitsCreationOptions
    {
        /// <summary>
        /// All bits in the bit mask are set to on (1)
        /// </summary>
        Filled,

        /// <summary>
        /// All bits in the bit mask are set to off (0)
        /// </summary>
        Unfilled
    }
}
