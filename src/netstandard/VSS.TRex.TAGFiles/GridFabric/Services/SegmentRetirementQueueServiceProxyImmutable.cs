using VSS.TRex.GridFabric.NodeFilters;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.TAGFiles.GridFabric.Services
{

  /// <summary>
  /// Class responsible for deploying the segment retirement queue service
  /// </summary>
  public class SegmentRetirementQueueServiceProxyImmutable : SegmentRetirementQueueServiceProxyBase
  {
        /// <summary>
        /// No-arg constructor that instantiates the Ignite instance, cluster, service and proxy members
        /// </summary>
        public SegmentRetirementQueueServiceProxyImmutable() : base(StorageMutability.Immutable, new PSNodeRoleBasedNodeFilter())
        {
        }
    }
}
