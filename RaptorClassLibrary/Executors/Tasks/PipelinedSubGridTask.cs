using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    public class PipelinedSubGridTask : TaskBase, ITask
    {
        public SubGridPipelineBase PipeLine { get; set; } = null;

        public PipelinedSubGridTask(long requestDescriptor, GridDataType gridDataType) : base(requestDescriptor, gridDataType)
        {
        }

        public override bool TransferResponse(object response)
        {
            if (PipeLine != null && !PipeLine.PipelineAborted /*&& PipeLine.OperationNode != null*/)
            {
                // PipeLine.OperationNode.AddSubGridToOperateOn(response);
                return true;
            }
            else
            {
                // TODO Add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format(' WARNING: TASPipelinedTask.TransferSubgridResponse: No pipeline available to submit grouped result for request %d (verb %s)',        
                //                                          [RequestDescriptor, RPCVerbName(ResponseToVerb)]), slmcDebug);
                return false;
            }
        }

        public override void Cancel()
        {
            if (PipeLine != null)
            {
                try
                {
                    try
                    {
                        PipeLine.Abort();
                    }
                    catch
                    {
                        // Just in case the pipeline commits suicide before other related tasks are
                        // cancelled (and so also inform the pipeline that it is cancelled), swallow
                        // any exception generated for the abort request.
                    }
                }
                finally
                {
                    PipeLine = null;
                }
            }
        }
    }
}
