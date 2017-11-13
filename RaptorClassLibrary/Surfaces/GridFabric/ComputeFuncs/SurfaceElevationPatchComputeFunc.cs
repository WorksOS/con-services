using Apache.Ignite.Core.Compute;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Surfaces.Executors;
using VSS.VisionLink.Raptor.Surfaces.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.Surfaces.GridFabric.ComputeFuncs
{
    [Serializable]
    public class SurfaceElevationPatchComputeFunc : IComputeFunc<SurfaceElevationPatchArgument, byte[] /*ClientHeightAndTimeLeafSubGrid*/>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public byte[] Invoke(SurfaceElevationPatchArgument arg)
        {
            try
            {
                Log.InfoFormat("CalculateDesignElevationPatchComputeFunc: Arg = {0}", arg);

                CalculateSurfaceElevationPatch Executor = new CalculateSurfaceElevationPatch(arg);

                return Executor.Execute().ToByteArray();
            }
            catch (Exception E)
            {
                Log.InfoFormat("Exception:", E);
                return null; // Todo .....
            }
        }

    }
}
