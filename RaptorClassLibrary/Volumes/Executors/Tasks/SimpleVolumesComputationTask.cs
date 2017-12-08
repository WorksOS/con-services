using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
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
        private SimpleVolumesCalculationsAggregator Aggregator = null;

        /// <summary>
        /// Constructor accepting a simple volumes aggregator that hardwires the expected grid data type to height
        /// </summary>
        /// <param name="Aggregator"></param>
        public SimpleVolumesComputationTask(SimpleVolumesCalculationsAggregator Aggregator) : base(-1, "", GridDataType.Height)
        {

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

            Aggregator.SummariseSubgridResult(response as ClientHeightLeafSubGrid[]);

            return true;
        }
    }
}
