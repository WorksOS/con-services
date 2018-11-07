namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The two end points of a stright line used for a profile calculation, defined in the cartesian grid coordinate system of the project
  /// </summary>
  /// 
  public class ProfileGridPoints
  {
    /// <summary>
    /// X ordinate of the first profile end point. Values are expressed in meters.
    /// </summary>
    /// 
    public double x1 { get; set; }

    /// <summary>
    /// Y ordinate of the first profile end point. Values are expressed in meters.
    /// </summary>
    /// 
    public double y1 { get; set; }

    /// <summary>
    /// X ordinate of the second profile end point. Values are expressed in meters.
    /// </summary>
    /// 
    public double x2 { get; set; }

    /// <summary>
    /// Y ordinate of the second profile end point. Values are expressed in meters.
    /// </summary>
    /// 
    public double y2 { get; set; }
  }
}
