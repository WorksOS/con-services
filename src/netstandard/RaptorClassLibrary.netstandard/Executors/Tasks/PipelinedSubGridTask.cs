using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Reflection;
using VSS.TRex.Types;

namespace VSS.TRex.Executors.Tasks
{
    /// <summary>
    /// A base class implementing activities that accept subgrids from a pipelined subgrid query process
    /// </summary>
    public class PipelinedSubGridTask : TaskBase 
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Primary task constructor
        /// </summary>
        /// <param name="requestDescriptor"></param>
        /// <param name="tRexNodeID"></param>
        /// <param name="gridDataType"></param>
        public PipelinedSubGridTask(Guid requestDescriptor, string tRexNodeID, GridDataType gridDataType) : base(requestDescriptor, tRexNodeID, gridDataType)
        {
        }

        /// <summary>
        /// Transfers a single subgrid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            if (PipeLine != null && !PipeLine.Aborted /*&& PipeLine.OperationNode != null*/)
            {
                // PipeLine.OperationNode.AddSubGridToOperateOn(response);
                return true;
            }

            Log.LogInformation($" WARNING: PipelinedSubGridTask.TransferSubgridResponse: No pipeline available to submit grouped result for request {RequestDescriptor}");
            return false;
        }

        /// <summary>
        /// Cancels the currently executing pipline by instructing it to abort
        /// </summary>
        public override void Cancel()
        {
            if (PipeLine == null)
            {
                return;
            }

            try
            {
                Log.LogDebug("WARNING: Aborting pipeline due to cancellation");
                PipeLine.Abort();
            }
            catch
            {
                // Just in case the pipeline commits suicide before other related tasks are
                // cancelled (and so also inform the pipeline that it is cancelled), swallow
                // any exception generated for the abort request.
            }
            finally
            {
                Log.LogInformation("Nulling pipeline reference");
                PipeLine = null;
            }
        }

        /// <summary>
        /// Transfers a single subgrid response from a query context into the task processing context
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public override bool TransferResponses(object[] responses)
        {
            if (PipeLine != null && !PipeLine.Aborted /*&& PipeLine.OperationNode != null*/)
            {
                // PipeLine.OperationNode.AddSubGridToOperateOn(response);
                return true;
            }

            Log.LogInformation($" WARNING: PipelinedSubGridTask.TransferSubgridResponse: No pipeline available to submit grouped result for request {RequestDescriptor}");

            return responses.All(TransferResponse);
        }
    }
}
