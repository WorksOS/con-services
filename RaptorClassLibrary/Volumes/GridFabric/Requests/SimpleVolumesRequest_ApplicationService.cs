using Apache.Ignite.Core.Compute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.Requests
{
    /// <summary>
    /// A request that may be issued to compute a volume
    /// </summary>
    public class SimpleVolumesRequest_ApplicationService : GenericASNodeRequest<SimpleVolumesRequestArgument, SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesResponse> 
    {
        /// <summary>
        /// Add specific behaviour here if needed
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public override SimpleVolumesResponse Execute(SimpleVolumesRequestArgument arg)
        {
            return base.Execute(arg);
        }
    }
}
