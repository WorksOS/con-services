using System;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
    /// <summary>
    /// The base closure/function that implements subgrid request processing on compute nodes.
    /// Note that the pipeline and compute function are operating in the same context and cooperate through
    /// the Task member on the instance
    /// </summary>
    public class SubGridsRequestComputeFuncAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
        where TSubGridsRequestArgument : SubGridsRequestArgument
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        /// <summary>
        /// The Task responsible for handling further processing of subgrid query responses
        /// </summary>
        public ITask Task { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridsRequestComputeFuncAggregative()
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
        /// <returns></returns>
        public override TSubGridRequestsResponse AcquireComputationResult()
        {
            return new TSubGridRequestsResponse();
        }

        /// <summary>
        /// Set up Ignite elements for aggregative subgrid requests
        /// </summary>
        public override bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse)
        {
            // No Ignite infrastructure required
            contextEstablishmentResponse = SubGridRequestsResponseResult.OK;
            return true;
        }
    }
}
