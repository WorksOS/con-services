using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.SubGrids.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The base closure/function that implements subgrid request processing on compute nodes
  /// </summary>
  public class SubGridsRequestComputeFuncProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridsRequestComputeFuncBase<TSubGridsRequestArgument, TSubGridRequestsResponse>
    where TSubGridsRequestArgument : SubGridsRequestArgument
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    protected override SubGridsRequestComputeFuncBase_Executor<TSubGridsRequestArgument, TSubGridRequestsResponse> GetExecutor()
    {
      return new SubGridsRequestComputeFuncBase_Executor_Progressive<TSubGridsRequestArgument, TSubGridRequestsResponse>();
    }
  }
}
