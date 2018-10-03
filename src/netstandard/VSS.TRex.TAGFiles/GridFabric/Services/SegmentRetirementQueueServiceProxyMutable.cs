using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.GridFabric.NodeFilters;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{

  /// <summary>
  /// Class responsible for deploying the segment retirement queue service
  /// </summary>
  public class SegmentRetirementQueueServiceProxyMutable : SegmentRetirementQueueServiceProxyBase
  {
        /// <summary>
        /// No-arg constructor that instantiates the Ignite instance, cluster, service and proxy members
        /// </summary>
        public SegmentRetirementQueueServiceProxyMutable() : base(StorageMutability.Mutable, new TAGProcessorRoleBasedNodeFilter())
        {
        }
    }
}
