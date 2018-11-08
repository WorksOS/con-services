using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// Represents result returned by levation Statistics request
  /// </summary>
  public class ElevationStatisticsResult : ContractExecutionResult
  {
    protected ElevationStatisticsResult(string message)
      : base(message)
    { }

    /// <summary>
    /// Private constructor
    /// </summary>
    private ElevationStatisticsResult()
    { }

    public void SwapElevationsIfRequired()
    {
      if (MinElevation > MaxElevation)
      {
        var tempVar = MinElevation;
        MinElevation = MaxElevation;
        MaxElevation = tempVar;
      }
    }

    /// <summary>
    /// Zone boundaries
    /// </summary>
    public BoundingBox3DGrid BoundingExtents { get; private set; }
    /// <summary>
    /// Minimum elevation of cells tht matched the filter. 
    /// </summary>
    public double MinElevation { get; private set; }
    /// <summary>
    /// Maximum elevation of cells tht matched the filter. 
    /// </summary>
    public double MaxElevation { get; private set; }
    /// <summary>
    /// Total coverage area (cut + fill + no change) in m2. 
    /// </summary>
    public double TotalCoverageArea { get; private set; }

    public static ElevationStatisticsResult CreateElevationStatisticsResult(BoundingBox3DGrid convertExtents, double minElevation,
            double maxElevation, double totalCoverageArea)
    {
      return new ElevationStatisticsResult
      {
        BoundingExtents = convertExtents,
        MinElevation = minElevation,
        MaxElevation = maxElevation,
        TotalCoverageArea = totalCoverageArea,
        Message = convertExtents == null ? "No elevation range" : DefaultMessage,
        Code = convertExtents == null ? ContractExecutionStatesEnum.FailedToGetResults : ContractExecutionStatesEnum.ExecutedSuccessfully
      };
    }
  }
}
