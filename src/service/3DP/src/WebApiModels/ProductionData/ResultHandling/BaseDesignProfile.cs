using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// Base class containing common information relevant to linear and alignment based profile calculations
  /// </summary>
  public abstract class BaseDesignProfile : ContractExecutionResult
  {
    /// <summary>
    /// Was the profile calculation successful?
    /// </summary>
    /// 
    public bool success;

    /// <summary>
    /// The minimum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double minStation;

    /// <summary>
    /// The maximum station value of information calculated along the length of the profile line/alignment. Station values are with respect to 
    /// the first position of the profile line or alignment.
    /// </summary>
    /// 
    public double maxStation;

    /// <summary>
    /// The minimum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double minHeight;

    /// <summary>
    /// The maximum elevation across all cells processed in the profile result
    /// </summary>
    /// 
    public double maxHeight;

    /// <summary>
    /// The grid distance between the two profile end points. For straight line profiles this is the geomtric plane distance between the points. 
    /// For alignment profiles this is the station distance between start and end locations on the alignment the profile is computed between.
    /// </summary>
    /// 
    public double gridDistanceBetweenProfilePoints;
  }
}