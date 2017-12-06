using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs
{
    public class SimpleVolumesRequestComputeFunc : BaseRaptorComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            throw new NotImplementedException();
        }
    }
}
