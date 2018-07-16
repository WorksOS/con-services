using System;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
    /// <summary>
    /// Argument containing the parameters required for a Cut/Fill statistics request
    /// </summary>    
    [Serializable]
    public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument
    {
        // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor;

        /// <summary>
        /// The set of cut/fill offsets
        /// Current this is always 7 elements in array and assumes grade is set at zero
        /// eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
        /// </summary>
        public double[] Offsets { get; set; }

        /// <summary>
        /// The ID of the design to compute cut fill values between it and the production data elevatoins
        /// </summary>
        public Guid DesignID { get; set; }
    }
}
