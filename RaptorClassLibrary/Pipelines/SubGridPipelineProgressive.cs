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
    public class SubGridPipelineProgressive : SubGridPipelineBase<SubGridRequestsProgressive, SubGridsRequestComputeFuncProgressive>
    {
        public SubGridPipelineProgressive(int AID, PipelinedSubGridTask task) : base(AID, task)
        {

        }
    }
}
