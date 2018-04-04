using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.Arguments
{
    /// <summary>
    /// Argument containing the parameters required for a Cut/Fill statistics request
    /// </summary>
    public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The project the request is relevant to
        /// </summary>
        public long DataModelID { get; set; }

        // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor;

        /// <summary>
        /// The filter to be used for the request
        /// </summary>
        public CombinedFilter Filter { get; set; }

        /// <summary>
        /// The set of cut/fill offsets
        /// Current this is always 7 elements in array and assumes grade is set at zero
        /// eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
        /// </summary>
        public double[] Offsets { get; set; }

        /// <summary>
        /// The ID of the design to compute cut fill values between it and the production data elevatoins
        /// </summary>
        public long DesignID { get; set; }

        // TODO  LiftBuildSettings  :TICLiftBuildSettings;
    }
}
