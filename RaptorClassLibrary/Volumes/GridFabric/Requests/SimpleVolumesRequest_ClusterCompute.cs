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
    public class SimpleVolumesRequest_ClusterCompute : GenericPSNodeBroadcastRequest<SimpleVolumesRequestArgument, SimpleVolumesRequestComputeFunc, SimpleVolumesResponse>
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

        /*
        The non-generic version of the execute method would look like this:

        /// <summary>
        /// Computes a simple volume according to the parameters in the request. 
        /// Execute2 is the literal implementation of the generic Execute method
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SimpleVolumesResponse Execute(SimpleVolumesRequestArgument arg)
        {
            // Construct the function to be used
            IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse> func = new SimpleVolumesRequestComputeFunc();

            // Broadcast the request to the compute pool and assembly a list of the results
            Task<ICollection<SimpleVolumesResponse>> taskResult = _Compute.BroadcastAsync(func, arg);

            // Reduce the set of results to a single volumes result and send the result back
            return taskResult.Result.Aggregate((first, second) => first.AggregateWith(second));
        }
        */
    }
}
