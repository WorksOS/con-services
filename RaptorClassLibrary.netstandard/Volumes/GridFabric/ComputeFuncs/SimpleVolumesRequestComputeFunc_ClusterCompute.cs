using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Reflection;
using VSS.TRex.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;
using VSS.TRex.Volumes.Executors;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Responses;

namespace VSS.TRex.Volumes.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The cimple volumes compute function that runs in the context of the cluster compute nodes. This function
    /// performs a volumes calculation across the paritions on this node only.
    /// </summary>
    public class SimpleVolumesRequestComputeFunc_ClusterCompute : BaseRaptorComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default no-arg constructor that orients the request to the available PSNODE servers on the immutable grid projection
        /// </summary>
        public SimpleVolumesRequestComputeFunc_ClusterCompute() : base(TRexGrids.ImmutableGridName(), ServerRoles.PSNODE)
        {
        }

        /// <summary>
        /// Invoke the simple volumes request locally on this node
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            Log.Info("In SimpleVolumesRequestComputeFunc_ClusterCompute.Invoke()");

            try
            {
                ComputeSimpleVolumes_Coordinator simpleVolumes = new ComputeSimpleVolumes_Coordinator
                    (arg.SiteModelID,
                     arg.VolumeType,
                     arg.BaseFilter,
                     arg.TopFilter,
                     arg.BaseDesignID,
                     arg.TopDesignID,
                     arg.AdditionalSpatialFilter,
                     arg.CutTolerance, 
                     arg.FillTolerance);

                Log.Info("Executing simpleVolumes.Execute()");

                return simpleVolumes.Execute();
            }
            finally
            {
                Log.Info("Exiting SimpleVolumesRequestComputeFunc_ClusterCompute.Invoke()");
            }
        }
    }
}
