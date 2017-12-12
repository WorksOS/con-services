using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Requests;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs
{
    /// <summary>
    /// Thi copmpute func operates inthe context of an application server that reaches out to the compute cluster to 
    /// perform subgrids processing.
    /// </summary>
    public class SimpleVolumesRequestComputeFunc_ApplicationService : BaseRaptorComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
