using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.GridFabric.Models.Responses;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Pipelines.Tasks;

namespace VSS.TRex.Pipelines
{
  /// <summary>
  /// Defines a generic class that decorates progressive pipeline semantics with the desired argument and request response
  /// </summary>
  /// <typeparam name="TSubGridsRequestArgument"></typeparam>
  /// <typeparam name="TSubGridRequestsResponse"></typeparam>
  public class SubGridPipelineProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridPipelineBase<
    TSubGridsRequestArgument, TSubGridRequestsResponse,
    SubGridRequestsProgressive<TSubGridsRequestArgument, TSubGridRequestsResponse>>
    where TSubGridsRequestArgument : SubGridsRequestArgument, new()
    where TSubGridRequestsResponse : SubGridRequestsResponse, new()
  {
    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public SubGridPipelineProgressive() : base()
    {
    }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    /// <param name="AID"></param>
    /// <param name="task"></param>
    public SubGridPipelineProgressive( /*int AID, */ PipelinedSubGridTask task) : base( /*AID,*/ task)
    {
    }
  }
}
