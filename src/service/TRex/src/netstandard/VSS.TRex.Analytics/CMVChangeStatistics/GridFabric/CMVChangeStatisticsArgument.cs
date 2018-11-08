using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV change statistics request.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>    
  public class CMVChangeStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// CMV change details values.
    /// </summary>
    public double[] CMVChangeDetailsDatalValues { get; set; }
  }
}
