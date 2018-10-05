using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  ///  Represents a request that can be made against the design profiler cluster group in the TRex grid
  /// </summary>
  public abstract class TAGFileProcessingPoolBinarizableRequest<ProcessTAGFileRequestArgument, ProcessTAGFileResponse> : BaseBinarizableRequest<ProcessTAGFileRequestArgument, ProcessTAGFileResponse>
  {
    /// <summary>
    /// Default no-arg constructor that sets up cluster and compute projections available for use by the TAG file processing pipeline
    /// </summary>
    public TAGFileProcessingPoolBinarizableRequest() : base(TRexGrids.MutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
    {
    }
  }
}
