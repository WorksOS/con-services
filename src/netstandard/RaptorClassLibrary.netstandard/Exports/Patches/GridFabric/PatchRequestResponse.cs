using System.Collections.Generic;
using VSS.TRex.Pipelines;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// The response returned from the Patches request executor that contains the response code and the set of
  /// subgrids extracted for the patch in question
  /// </summary>
  public class PatchRequestResponse : SubGridsPipelinedReponseBase
  {
    /// <summary>
    /// The total number of pages of subgrids required to contain the maximum number of subgrids'
    /// that may be retuned for the query
    /// </summary>
    public int TotalNumberOfPagesToCoverFilteredData { get; set; }
  
    /// <summary>
    /// The set of subgrids matching the filters and patch page requested
    /// </summary>
    public List<IClientLeafSubGrid> SubGrids { get; set; }
  }
}
