using System;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.SubGrids.Executors
{ 
  /// <summary>
  /// The closure/function that implements sub grid request processing on compute nodes
  /// </summary>
  public class SubGridsRequestComputeFuncBase_Executor_Aggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> :
                  SubGridsRequestComputeFuncBase_Executor_Base<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    // private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridsRequestComputeFuncBase_Executor_Aggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>>();

    /// <summary>
    /// The Task responsible for handling further processing of sub grid query responses
    /// </summary>
    public ITRexTask Task { get; set; }

    /// <summary>
    /// Processes a sub grid result consisting of a client leaf sub grid matching each of the filters present in the request
    /// </summary>
    /// <param name="results"></param>
    /// <param name="resultCount"></param>
    protected override void ProcessSubGridRequestResult(IClientLeafSubGrid[][] results, int resultCount)
    {
      if (Task == null)
      {
        throw new ArgumentException("Task null in ProcessSubGridRequestResult() for SubGridsRequestComputeFuncAggregative<TArgument, TResponse> instance.");
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
    /// Set up Ignite elements for aggregated sub grid requests
    /// </summary>
    protected override bool EstablishRequiredIgniteContext(out SubGridRequestsResponseResult contextEstablishmentResponse)
    {
      // No Ignite infrastructure required
      contextEstablishmentResponse = SubGridRequestsResponseResult.OK;
      return true;
    }
  }
}
