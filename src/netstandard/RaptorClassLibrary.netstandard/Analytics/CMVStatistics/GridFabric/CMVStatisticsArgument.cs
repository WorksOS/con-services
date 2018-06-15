using System;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV statistics request
  /// </summary>    
  [Serializable]
  public class CMVStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    // TODO If desired: ExternalDescriptor :TASNodeRequestDescriptor, which should be moved to the base class;

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
