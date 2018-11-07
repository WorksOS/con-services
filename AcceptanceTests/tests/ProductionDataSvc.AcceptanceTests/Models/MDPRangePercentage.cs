namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// Contains a percentage range of observed MDP values with respect to the target MDP value configured on a machine
  /// This is copied from ...\RaptorServicesCommon\Models\MDPRangePercentage.cs 
  /// </summary>
  public class MDPRangePercentage
  {
    /// <summary>
    /// The minimum percentage range. Must be between 0 and 250.
    /// </summary>
    public double min { get; set; }

    /// <summary>
    /// The maximum percentage range. Must be between 0 and 250.
    /// </summary>
    public double max { get; set; }
  }
}