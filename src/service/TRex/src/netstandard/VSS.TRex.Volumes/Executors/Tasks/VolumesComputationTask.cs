using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Volumes.Executors.Tasks
{
    /// <summary>
    /// A pipelined sub grid task that accepts pairs of processed client height grids and compares them to compute volume
    /// information using the supplied aggregator.
    /// </summary>
    public class VolumesComputationTask : PipelinedSubGridTask
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<VolumesComputationTask>();

        /// <summary>
        /// The aggregator performing volumes computation operations
        /// </summary>
        public ISubGridRequestsAggregator Aggregator;

        /// <summary>
        /// Constructor that hard wires the expected grid data type to height
        /// </summary>
        public VolumesComputationTask() 
        {
          GridDataType = GridDataType.Height;
        }

        /// <summary>
        /// Receives a pair of sub grids from the sub grid compute engine and passes them to the volumes aggregator for summation
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public override bool TransferResponse(object response)
        {
            // Log.InfoFormat("Received a SubGrid to be processed: {0}", (response as IClientLeafSubGrid).Moniker());

            var result = false;

            if (base.TransferResponse(response))
            {
              if (Aggregator == null)
              {
                throw new TRexException("Aggregator not defined in VolumesComputationTask");
              }

              if (!(response is IClientLeafSubGrid[][] responseSubGrids))
              {
                Log.LogError($"Response is not a IClientLeafSubGrid[][], --> {response}");
              }
              else
              {
                // Include this sub grid result into the aggregated volumes result
                Aggregator.ProcessSubGridResult(responseSubGrids);
                result = true;
              }
            }

            return result;
        }
    }
}
