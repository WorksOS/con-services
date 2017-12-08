using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// Denotes the different types of volume conputations that may be performed
    /// </summary>
    public enum VolumeComputationType
    {
        None,
        AboveLevel,
        Between2Levels,
        AboveFilter,
        Between2Filters,
        BetweenFilterAndDesign,
        BetweenDesignAndFilter
    }
}
