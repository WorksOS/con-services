using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Executors.Tasks
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
        private ISubGridRequestsAggregator Aggregator = null;

        /// <summary>
        /// Constructor acceoting an aggregator and defaulting all other internal Task state
        /// </summary>
        /// <param name="aggregator"></param>
        public AggregatedPipelinedSubGridTask(ISubGridRequestsAggregator aggregator) : base(Guid.NewGuid().GetHashCode(), "", GridDataType.All)
        {
            Aggregator = aggregator;
        }
    }
}
