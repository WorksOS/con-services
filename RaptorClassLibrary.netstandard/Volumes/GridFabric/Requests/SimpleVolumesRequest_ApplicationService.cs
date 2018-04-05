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
    }
}
