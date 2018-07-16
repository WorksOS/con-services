using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.ComputeFuncs;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.Requests
{
    /// <summary>
    /// A request that may be issued to compute a volume
    /// </summary>
    public class SimpleVolumesRequest_ApplicationService : GenericASNodeRequest<SimpleVolumesRequestArgument, SimpleVolumesRequestComputeFunc_ApplicationService, SimpleVolumesResponse> 
    {
    }
}
