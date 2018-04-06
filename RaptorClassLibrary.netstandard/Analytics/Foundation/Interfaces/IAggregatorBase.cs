using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;

namespace VSS.VisionLink.Raptor.Analytics.Interfaces
{
    /// <summary>
    /// Interface for state aggrgator
    /// </summary>
    public interface IAggregatorBase
    {
        /// <summary>
        /// Performs aggrator summarisation business logic over a set of subgrids derived from the query engine
        /// </summary>
        /// <param name="subGrids"></param>
        void SummariseSubgridResult(IClientLeafSubGrid[][] subGrids);
    }
}
