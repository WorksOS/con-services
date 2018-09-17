using VSS.TRex.GridFabric.Models.Arguments;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV change statistics request
  /// </summary>    
  public class CMVChangeStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// CMV change details values.
    /// </summary>
    public double[] CMVChangeDetailsDatalValues { get; set; }
  }
}
