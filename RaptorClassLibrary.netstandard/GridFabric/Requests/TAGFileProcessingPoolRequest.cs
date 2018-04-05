using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Arguments;
using VSS.VisionLink.Raptor.TAGFiles.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    ///  Represents a request that can be made against the design profiler cluster group in the Raptor grid
    /// </summary>
    public class TAGFileProcessingPoolRequest : BaseRaptorRequest<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
    {
        /// <summary>
        /// Default no-arg constructor that sets up cluster and compute projections available for use by the TAG file processing pipeline
        /// </summary>
        public TAGFileProcessingPoolRequest() : base(RaptorGrids.RaptorMutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
        {
        }
    }
}
