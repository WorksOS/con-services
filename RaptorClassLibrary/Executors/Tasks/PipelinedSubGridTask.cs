using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
{
    /// <summary>
    /// A base class implementing activities that accept subgrids from a pipelined subgrid query process
    /// </summary>
    public class PipelinedSubGridTask : TaskBase, ITask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public PipelinedSubGridTask(long requestDescriptor, string raptorNodeID, GridDataType gridDataType) : base(requestDescriptor, raptorNodeID, gridDataType)
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
                Log.InfoFormat(" WARNING: PipelinedSubGridTask.TransferSubgridResponse: No pipeline available to submit grouped result for request {0}", RequestDescriptor);
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
                        Log.Debug("WARNING: Aborting pipeline due to cancellation");
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
                    Log.Info("Nulling pipeline reference");
                    PipeLine = null;
                }
            }
        }
    }
}
