using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Requests;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Defines a generic class that decorates aggregative pipeline semantics with the desired argument and request response
  /// </summary>
  /// <typeparam name="TSubGridsRequestArgument"></typeparam>
  /// <typeparam name="TSubGridRequestsResponse"></typeparam>
  public class SubGridPipelineAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : 
    SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>>
    where TSubGridsRequestArgument : SubGridsRequestArgument, new()
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridPipelineAggregative() : base()
    {
    }

    /// <summary>
    /// Creates a pip
    /// </summary>
    /// <param name="task"></param>
    public SubGridPipelineAggregative( /*int AID, */ ITRexTask task) : base( /*AID, */ task)
    {
    }
  }
}
