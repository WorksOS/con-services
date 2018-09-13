using System;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a MDP statistics request
  /// </summary>    
  [Serializable]
  public class MDPStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The flag is to indicate wehther or not the machine MDP target to be user overrides.
    /// </summary>
    public bool OverrideMachineMDP { get; set; }

    /// <summary>
    /// User overriding MDP target value.
    /// </summary>
    public short OverridingMachineMDP { get; set; }

    /// <summary>
    /// MDP percentage range.
    /// </summary>
    public MDPRangePercentageRecord MDPPercentageRange;

    /// <summary>
    /// MDP details values.
    /// </summary>
    public int[] MDPDetailValues { get; set; }
  }
}
