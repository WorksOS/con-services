using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.Velociraptor.DesignProfiling.Executors;
using log4net;
using System.Reflection;

namespace VSS.Velociraptor.DesignProfiling.GridFabric.ComputeFuncs
{
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : IComputeFunc<CalculateDesignElevationPatchArgument, ClientHeightLeafSubGrid>
    {
        [NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ClientHeightLeafSubGrid Invoke(CalculateDesignElevationPatchArgument arg)
        {
            try
            {
                Log.InfoFormat("CalculateDesignElevationPatchComputeFunc: Arg = {0}", arg);

                CalculateDesignElevationPatch Executor = new CalculateDesignElevationPatch(arg);

                return Executor.Execute();
            }
            catch
            {
                return null; // Todo .....
            }
        }
    }
}
