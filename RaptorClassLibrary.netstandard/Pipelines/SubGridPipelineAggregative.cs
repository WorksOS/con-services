using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Pipelines
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
