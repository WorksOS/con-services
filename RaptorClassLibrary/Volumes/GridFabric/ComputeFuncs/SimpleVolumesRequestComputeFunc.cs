using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Volumes.Executors;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs
{
    public class SimpleVolumesRequestComputeFunc : BaseRaptorComputeFunc, IComputeFunc<SimpleVolumesRequestArgument, SimpleVolumesResponse>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SimpleVolumesResponse Invoke(SimpleVolumesRequestArgument arg)
        {
            Log.Info("In SimpleVolumesRequestComputeFunc.Invoke()");

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
                Log.Info("Exiting SimpleVolumesRequestComputeFunc.Invoke()");
            }
        }
    }
}
