using System;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The closure/function that implements subgrid request processing on compute nodes
  /// </summary>
  public class SubGridsRequestComputeFuncBase_Executor_Aggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> :
                  SubGridsRequestComputeFuncBase_Executor<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    // private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase_Executor_Aggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// The Task responsible for handling further processing of subgrid query responses
    /// </summary>
    public ITRexTask Task { get; set; }

    /// <summary>
    /// Processes a subgrid result consisting of a client leaf subgrid matching each of the filters present in the request
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultCount"></param>
    protected override void ProcessSubgridRequestResult(IClientLeafSubGrid[][] results, int resultCount)
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
    protected override TSubGridRequestsResponse AcquireComputationResult()
    {
      return new TSubGridRequestsResponse();
    }

    /// <summary>
    /// Set up Ignite elements for aggregated subgrid requests
    /// </summary>
    protected override bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse)
    {
      // No Ignite infrastructure required
      contextEstablishmentResponse = SubGridRequestsResponseResult.OK;
      return true;
    }
  }
}
