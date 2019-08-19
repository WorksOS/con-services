using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Result of request to get production data edits
  /// </summary>
  public class TRexEditDataResult : ContractExecutionResult
  {
    /// <summary>
    /// The collection of data edits applied to the production data.
    /// </summary>
    public List<TRexEditData> DataEdits { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private TRexEditDataResult()
    { }

    /// <summary>
    /// Create instance of EditDataResult
    /// </summary>
    public TRexEditDataResult(List<TRexEditData> dataEdits)
    {
      DataEdits = dataEdits;
    }
  }
}
