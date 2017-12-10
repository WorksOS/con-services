using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    /// <summary>
    /// Performs subgrid requests where the procesing result is aggregated and returned as a set of paritioned responses
    /// from the cache computer cluster
    /// </summary>
    public class SubGridRequestsAggregative : SubGridRequestsBase<SubGridsRequestComputeFuncAggregative>
    {
        /// <summary>
        /// Default no-arg constructor thje delgates construction to the base class
        /// </summary>
        public SubGridRequestsAggregative() : base()
        {
        }

        /// <summary>
        /// Overrides the base Execut() semantics to add a listener available for aggregative processing of subgrids in the request engine.
        /// </summary>
        /// <returns></returns>
        public override ICollection<SubGridRequestsResponse> Execute()
        {
            PrepareArgument();

            return base.Execute();
        }
    }
}
