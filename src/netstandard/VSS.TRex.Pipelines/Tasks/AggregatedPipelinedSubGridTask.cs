using System;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Pipelines.Tasks
{
    /// <summary>
    /// Provides pipelined task semantics for workflows where the subgrids being processed are being aggregated into
    /// a summarised version rather than being passed through to the requesting context
    /// </summary>
    public class AggregatedPipelinedSubGridTask : PipelinedSubGridTask
    {
        /// <summary>
        /// The aggregator performing computation operations
        /// </summary>
        private ISubGridRequestsAggregator Aggregator;

        /// <summary>
        /// Constructor acceoting an aggregator and defaulting all other internal Task state
        /// </summary>
        /// <param name="aggregator"></param>
        public AggregatedPipelinedSubGridTask(ISubGridRequestsAggregator aggregator) : base(Guid.NewGuid(), "", Types.GridDataType.All)
        {
            Aggregator = aggregator;
        }

        /// <summary>
        /// Transfers a single subgrid response from a query context into the task processing context
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            if (base.TransferResponse(response))
            {
                Aggregator.ProcessSubgridResult(response as IClientLeafSubGrid[][]);
                return true;
            }

            return false;
        }
    }
}
