using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Coords;

namespace VSS.Productivity3D.Models.ResultHandling.Coords
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
    public TwoDConversionCoordinate[] ConversionCoordinates { get; private set; }

    /// <summary>
    /// Dewfault private constructor.
    /// </summary>
    /// 
    private CoordinateConversionResult()
    {
      // ...
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="conversionCoordinates">Array of conversion coordinates.</param>
    /// <returns>A created instance of the CoordinateConversionResult class.</returns>
    /// 
    public CoordinateConversionResult(TwoDConversionCoordinate[] conversionCoordinates)
    {
      ConversionCoordinates = conversionCoordinates;
    }
  }
}