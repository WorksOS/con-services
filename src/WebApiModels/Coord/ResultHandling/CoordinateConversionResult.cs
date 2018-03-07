using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApiModels.Coord.Models;

namespace VSS.Productivity3D.WebApiModels.Coord.ResultHandling
{
  /// <summary>
  /// Coordinate conversion result class.
  /// </summary>
  ///
  public class CoordinateConversionResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of converted coordinates.
    /// </summary>
    /// 
    public TwoDConversionCoordinate[] conversionCoordinates { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private CoordinateConversionResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CoordinateConversionResult class.
    /// </summary>
    /// <param name="conversionCoordinates">Array of conversion coordinates.</param>
    /// <returns>A created instance of the CoordinateConversionResult class.</returns>
    /// 
    public static CoordinateConversionResult CreateCoordinateConversionResult(TwoDConversionCoordinate[] conversionCoordinates)
    {
      return new CoordinateConversionResult { conversionCoordinates = conversionCoordinates };
    }
  }
}