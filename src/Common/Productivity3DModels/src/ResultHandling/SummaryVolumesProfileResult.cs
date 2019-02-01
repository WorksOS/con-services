using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes Profile request
  /// </summary>
  public class SummaryVolumesProfileResult : ContractExecutionResult
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SummaryVolumesProfileResult()
    { }

    // Todo Make a return class
  //  public List<SummaryVolumeProfileCell> ProfileCells { get; set; } = new List<SummaryVolumeProfileCell>();

    public bool HasData() => true;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static SummaryVolumesProfileResult Create()
    {
      // todo add params etc
      return new SummaryVolumesProfileResult
      {
      };
      
    }
  }
}
