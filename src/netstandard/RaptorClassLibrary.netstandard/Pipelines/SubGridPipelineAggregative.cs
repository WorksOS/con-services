using VSS.TRex.Executors.Tasks;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.GridFabric.Responses;

namespace VSS.TRex.Pipelines
{
    /// <summary>
    /// Defines a generic class that decorates aggregative pipeline semantics with the desired argument and request response
    /// </summary>
    /// <typeparam name="TSubGridsRequestArgument"></typeparam>
    /// <typeparam name="TSubGridRequestsResponse"></typeparam>
    public class SubGridPipelineAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse> : SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, 
        SubGridRequestsAggregative<TSubGridsRequestArgument, TSubGridRequestsResponse>>
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
    {
        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="task"></param>
        public SubGridPipelineAggregative(/*int AID, */ PipelinedSubGridTask task) : base(/*AID, */ task)
        {
        }
    }
}
