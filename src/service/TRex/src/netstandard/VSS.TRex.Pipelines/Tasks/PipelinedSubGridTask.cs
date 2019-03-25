using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.TRex.Types;

namespace VSS.TRex.Pipelines.Tasks
{
    /// <summary>
    /// A base class implementing activities that accept sub grids from a pipelined sub grid query process
    /// </summary>
    public class PipelinedSubGridTask : TaskBase 
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<PipelinedSubGridTask>();

        public PipelinedSubGridTask()
        {
        }

        /// <summary>
        /// Transfers a single sub grid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            bool result = PipeLine != null && !PipeLine.Aborted;

            if (!result)
              Log.LogInformation($" WARNING: PipelinedSubGridTask.TransferSubGridResponse: No pipeline available to submit grouped result for request {RequestDescriptor}");

            return result;
        }

        /// <summary>
        /// Cancels the currently executing pipeline by instructing it to abort
        /// </summary>
        public override void Cancel()
        {
          if (PipeLine != null)
          {
            try
            {
              Log.LogDebug("WARNING: Aborting pipeline due to cancellation");
              PipeLine.Abort();
            }
            catch (Exception e)
            {
              Log.LogError(e, "Exception occurred during pipeline cancellation");
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
        }

        /// <summary>
        /// Transfers a single sub grid response from a query context into the task processing context
        /// </summary>
        /// <param name="responses"></param>
        /// <returns></returns>
        public override bool TransferResponses(object[] responses)
        {
          bool result = PipeLine != null && !PipeLine.Aborted;

          if (!result)
            Log.LogInformation($" WARNING: {nameof(TransferResponses)}: No pipeline available to submit grouped result for request {RequestDescriptor}");

          return result && responses.All(TransferResponse);
        }
    }
}
