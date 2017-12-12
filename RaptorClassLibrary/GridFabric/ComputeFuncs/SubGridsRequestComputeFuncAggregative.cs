using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks.Interfaces;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base closure/function that implements subgrid request processing on compute nodes.
    /// Note that the pipeline and compute function are operating in the same context and cooperate through
    /// the Task member on the instance
    /// </summary>
    [Serializable]
    public class SubGridsRequestComputeFuncAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        /// <summary>
        /// The Task responsible for handling further processing of subgrid query responses
        /// </summary>
        public ITask Task { get; set; } = null;

        /// <summary>
        /// Default no-arg construcor
        /// </summary>
        public SubGridsRequestComputeFuncAggregative() : base()
        {
        }

        /// <summary>
        /// Processes a subgrid result consisting of a client leaf subgrid matching each of the filters present in the request
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultCount"></param>
        public override void ProcessSubgridRequestResult(IClientLeafSubGrid[][] results, int resultCount)
        {       
            if (Task == null)
            {
                throw new ArgumentException("Task null in ProcessSubgridRequestResult() for SubGridsRequestComputeFuncAggregative<TArgument, TResponse> instance.");
            }

            Task.TransferResponse(results);
        }

        /// <summary>
        /// Transforms the internal aggregation state into the desired response for the request
        /// </summary>
        /// <param name="results"></param>
        /// <param name="resultCount"></param>
        /// <returns></returns>
        public override TSubGridRequestsResponse AcquireComputationResult()
        {
            return new TSubGridRequestsResponse();
        }

    }
}
