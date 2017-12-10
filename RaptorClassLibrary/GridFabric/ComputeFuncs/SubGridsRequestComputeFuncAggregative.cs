using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base closure/function that implements subgrid request processing on compute nodes
    /// </summary>
    [Serializable]
    public class SubGridsRequestComputeFuncAggregative : SubGridsRequestComputeFuncBase
    {
        public override void ProcessSubgridRequestResult(IClientLeafSubGrid[][] results, int resultCount)
        {
            base.ProcessSubgridRequestResult(results, resultCount);
        }
    }
}
