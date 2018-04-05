using log4net;
using System;
using System.Reflection;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Volumes.Executors.Tasks
{
    /// <summary>
    /// A pipelined subgrid task that accepts pairs of processed client height grids and compares them to compute volume
    /// informatikon using the supplied aggregator.
    /// </summary>
    public class SimpleVolumesComputationTask : PipelinedSubGridTask
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The aggregator performing volumes computation operations
        /// </summary>
        private ISubGridRequestsAggregator Aggregator;

        /// <summary>
        /// Constructor accepting a simple volumes aggregator that hardwires the expected grid data type to height
        /// </summary>
        /// <param name="aggregator"></param>
        public SimpleVolumesComputationTask(ISubGridRequestsAggregator aggregator) : base(Guid.NewGuid().GetHashCode(), "", GridDataType.Height)
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
                Log.Warn("Base TransferResponse returned false");
                return false;
            }

            if (Aggregator == null)
            {
                throw new ArgumentException("Aggregator not defined in SimpleVolumesComputationTask");
            }

            if (!(response is IClientLeafSubGrid[][]))
            {
                Log.Error($"response is not a IClientLeafSubGrid[][], --> {response}");
                return false;
            }

            // Include this subgrid result into the aggregated volumes result
            Aggregator.ProcessSubgridResult(response as IClientLeafSubGrid[][]);

            return true;
        }
    }
}
