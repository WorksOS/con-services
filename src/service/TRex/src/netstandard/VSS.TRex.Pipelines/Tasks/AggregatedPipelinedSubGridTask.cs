using System;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Pipelines.Tasks
{
  /// <summary>
  /// Provides pipelined task semantics for work flows where the sub grids being processed are being aggregated into
  /// a summarized version rather than being passed through to the requesting context
  /// </summary>
  public class AggregatedPipelinedSubGridTask : PipelinedSubGridTask, IAggregatedPipelinedSubGridTask
  {
        /// <summary>
        /// The aggregator performing computation operations
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; }
      
        public AggregatedPipelinedSubGridTask() : base(Guid.NewGuid(), "", Types.GridDataType.All)
        {
        }

        /// <summary>
        /// Transfers a single sub grid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            bool result = false;

            if (base.TransferResponse(response))
            {
                Aggregator.ProcessSubGridResult(response as IClientLeafSubGrid[][]);
                result = true;
            }

            return result;
        }
    }
}
