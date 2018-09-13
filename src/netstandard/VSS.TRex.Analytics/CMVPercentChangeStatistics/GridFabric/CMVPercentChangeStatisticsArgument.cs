using VSS.TRex.GridFabric.Models.Arguments;

namespace VSS.TRex.Analytics.CMVPercentChangeStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV % change statistics request
  /// </summary>    
  public class CMVPercentChangeStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// CMV % change details values.
    /// </summary>
    public double[] CMVPercentChangeDatalValues { get; set; }
  }
}
