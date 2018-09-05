using System;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Summary
{
  /// <summary>
  /// Argument containing the parameters required for a CMV summary request
  /// </summary>    
  [Serializable]
  public class CMVSummaryArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine CMV target to be user overrides.
    /// </summary>
    public bool OverrideMachineCMV { get; set; }

    /// <summary>
    /// User overriding CMV target value.
    /// </summary>
    public short OverridingMachineCMV { get; set; }

    /// <summary>
    /// CMV percentage range.
    /// </summary>
    public CMVRangePercentageRecord CMVPercentageRange;
  }
}
