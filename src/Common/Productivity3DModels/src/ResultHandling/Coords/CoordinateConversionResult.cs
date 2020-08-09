using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Coords;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
{
  /// <summary>
  /// Coordinate conversion result class.
  /// </summary>
  public class CoordinateConversionResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of converted coordinates.
    /// </summary>
    public TwoDConversionCoordinate[] ConversionCoordinates { get; private set; }

    private CoordinateConversionResult()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public CoordinateConversionResult(TwoDConversionCoordinate[] conversionCoordinates)
    {
      ConversionCoordinates = conversionCoordinates;
    }

    public void SetConversionCoordinates(TwoDConversionCoordinate[] conversionCoordinates) => ConversionCoordinates = conversionCoordinates;
  }
}
