using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
    /// <summary>
    /// This compute func operates in the context of an application server that reaches out to the compute cluster to 
    /// perform subgrid processing.
    /// </summary>
    public class SimpleVolumesRequestComputeFunc_ApplicationService : BaseComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available ASNODE servers on the immutable grid projection
        /// </summary>
        public SimpleVolumesRequestComputeFunc_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
        {
        }

        /// <summary>
        /// Invokes the simple volumes request with the given simple volumes request argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            Log.LogInformation("In SimpleVolumesRequestComputeFunc_ApplicationService.Invoke()");

            try
            {
                SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

                Log.LogInformation("Executing SimpleVolumesRequestComputeFunc_ApplicationService.Execute()");

                return request.Execute(arg);
            }
            finally
            {
                Log.LogInformation("Exiting SimpleVolumesRequestComputeFunc_ApplicationService.Invoke()");
            }
        }
    }
}
