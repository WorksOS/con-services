using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using VSS.Velociraptor.DesignProfiling.GridFabric.Arguments;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.Velociraptor.DesignProfiling.Executors;

namespace VSS.Velociraptor.DesignProfiling.GridFabric.ComputeFuncs
{
    [Serializable]
    public class CalculateDesignElevationPatchComputeFunc : IComputeFunc<CalculateDesignElevationPatchArgument, ClientHeightLeafSubGrid>
    {
        public ClientHeightLeafSubGrid Invoke(CalculateDesignElevationPatchArgument arg)
        {
            try
            {
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
