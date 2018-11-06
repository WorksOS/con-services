using VSS.TRex.CoordinateSystems.Models;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// DTO response object returned from Coordinate Service endpoint; PUT: /coordinatesystems/imports/dc/file
  /// </summary>
  /// <remarks>
  /// See Swagger definition:
  /// (staging): https://api-stg.trimble.com/t/trimble.com/coordinates/1.0/swagger/ui/index#!/CoordinateSystems/CoordinateSystems_GetFromDC
  /// (prod): https://api.trimble.com/t/trimble.com/coordinates/1.0/swagger/ui/index#!/CoordinateSystems/CoordinateSystems_GetFromDC
  /// </remarks>
  public struct CoordinateSystemResponse
  {
    public CoordinateSystem CoordinateSystem;
    public int[] CSIB;
  }
}
