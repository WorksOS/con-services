using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;

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
      private ITRexTask _task;

      protected override SubGridsRequestComputeFuncBase_Executor<TSubGridsRequestArgument, TSubGridRequestsResponse> GetExecutor()
      {
        return new SubGridsRequestComputeFuncBase_Executor_Aggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>()
        {
          Task = _task
        };
      }

      public SubGridsRequestComputeFuncAggregative(ITRexTask task)
      {
        _task = task;
      }
    }
}
