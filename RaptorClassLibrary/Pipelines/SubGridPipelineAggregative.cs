using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Requests;

namespace VSS.VisionLink.Raptor.Pipelines
{
    public class SubGridPipelineAggregative : SubGridPipelineBase<SubGridRequestsAggregative, SubGridsRequestComputeFuncAggregative>
    {
        public SubGridPipelineAggregative(int AID, PipelinedSubGridTask task) : base(AID, task)
        {

        }
    }
}
