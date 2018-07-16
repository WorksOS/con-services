using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Volumes.Executors.Tasks
{
    /// <summary>
    /// A pipelined subgrid task that accepts pairs of processed client height grids and compares them to compute volume
    /// informatikon using the supplied aggregator.
    /// </summary>
    public class SimpleVolumesComputationTask : PipelinedSubGridTask
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The aggregator performing volumes computation operations
        /// </summary>
        private ISubGridRequestsAggregator Aggregator;

        /// <summary>
        /// Constructor accepting a simple volumes aggregator that hardwires the expected grid data type to height
        /// </summary>
        /// <param name="aggregator"></param>
        public SimpleVolumesComputationTask(ISubGridRequestsAggregator aggregator) : base(Guid.NewGuid(), "", GridDataType.Height)
        {
            Aggregator = aggregator;
        }

        /// <summary>
        /// Receives a pait of subgrids from the subgrid compute engine and passes them to the simple volumes aggregator for summarisation
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            if (!base.TransferResponse(response))
            {
                Log.LogWarning("Base TransferResponse returned false");
                return false;
            }

            if (Aggregator == null)
            {
                throw new ArgumentException("Aggregator not defined in SimpleVolumesComputationTask");
            }

            if (!(response is IClientLeafSubGrid[][]))
            {
                Log.LogError($"response is not a IClientLeafSubGrid[][], --> {response}");
                return false;
            }

            // Include this subgrid result into the aggregated volumes result
            Aggregator.ProcessSubgridResult(response as IClientLeafSubGrid[][]);

            return true;
        }
    }
}
