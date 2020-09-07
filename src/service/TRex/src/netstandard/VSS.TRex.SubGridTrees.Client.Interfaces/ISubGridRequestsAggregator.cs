using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Interfaces
{
  /// <summary>
  /// Interface supporting SubGridRequestors performing aggregative processing of a set of sub grids in a request
  /// </summary>
  public interface ISubGridRequestsAggregator
  {
    /// <summary>
    /// Process the result of querying a sub grid against one or more filters. The argument is a generic list of client sub grids
    /// </summary>
    void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids);

    /// <summary>
    /// Perform any finalisation logic required once all sub grids have been processed into the aggregator
    /// </summary>
    void Finalise();
  }
}
