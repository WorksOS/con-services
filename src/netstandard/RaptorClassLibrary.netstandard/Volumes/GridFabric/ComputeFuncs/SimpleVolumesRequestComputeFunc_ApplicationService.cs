using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;
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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            Log.Info("In SimpleVolumesRequestComputeFunc_ApplicationService.Invoke()");

            try
            {
                SimpleVolumesRequest_ClusterCompute request = new SimpleVolumesRequest_ClusterCompute();

                Log.Info("Executing SimpleVolumesRequestComputeFunc_ApplicationService.Execute()");

                return request.Execute(arg);
            }
            finally
            {
                Log.Info("Exiting SimpleVolumesRequestComputeFunc_ApplicationService.Invoke()");
            }
        }
    }
}
