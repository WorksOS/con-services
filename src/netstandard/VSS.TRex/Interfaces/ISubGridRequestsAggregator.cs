using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Interfaces
{
    /// <summary>
    /// Interface supporting SubGridRequestors performing aggregative processing of a set of subgrids in a request
    /// </summary>
    public interface ISubGridRequestsAggregator
    {
        /// <summary>
        /// Process the result of querying a subgrid against one or more filters. The argument is a generic list of client subgrids
        /// </summary>
        /// <param name="subGrids"></param>
        void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids);

        /// <summary>
        /// Perform any finalisation logic required once all subgrids have been processed into the aggregator
        /// </summary>
        void Finalise();
    }
}
