using VSS.TRex.Common.Models;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  public interface IConvertCoordinates
  {
    /// <summary>
    /// Provides a null conversion between the 2D coordinates in a WGS84 LL point and a XYZ NEE point.
    /// Only the 2D coordinates are used, and directly copied from the LL point to the XY point, maintaining the
    /// X == Longitude and Y == Latitude sense of the relative coordinates
    /// </summary>
    XYZ NullWGSLLToXY(WGS84Point WGSLL);

    /// <summary>
    /// Takes an array of <see cref="WGS84Point"/> and uses the Coordinate Service to convert it into <see cref="XYZ"/> data.
    /// </summary>
    XYZ[] NullWGSLLToXY(WGS84Point[] WGSLLs);

    /// <summary>
    /// Takes a <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    NEE LLHToNEE(string id, LLH LLH, bool convertToRadians = true);

    /// <summary>
    /// Takes an array of <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    (RequestErrorStatus ErrorCode, NEE[] NEECoordinates) LLHToNEE(string id, LLH[] LLH, bool convertToRadians = true);

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="LLH"/> data into <see cref="NEE"/> data.
    /// </summary>
    XYZ LLHToNEE(string id, XYZ coordinates, bool convertToRadians = true);

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="NEE"/> data into <see cref="LLH"/> data.
    /// </summary>
    XYZ NEEToLLH(string id, XYZ coordinates);

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    (RequestErrorStatus ErrorCode, XYZ[] NEECoordinates) LLHToNEE(string id, XYZ[] coordinates, bool convertToRadians = true);

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    (RequestErrorStatus ErrorCode, XYZ[] LLHCoordinates) NEEToLLH(string id, XYZ[] coordinates);

    /// <summary>
    /// Takes a <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    LLH NEEToLLH(string id, NEE NEE);

    /// <summary>
    /// Takes an array of <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    (RequestErrorStatus ErrorCode, LLH[] LLHCoordinates) NEEToLLH(string id, NEE[] NEE);

    /// <summary>
    /// Uses the Coordinate Service to convert WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    XYZ WGS84ToCalibration(string id, WGS84Point wgs84Point, bool convertToRadians = true);

    /// <summary>
    /// Uses the Coordinate Service to convert an array of WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    XYZ[] WGS84ToCalibration(string id, WGS84Point[] wgs84Points, bool convertToRadians = true);

    /// <summary>
    /// Takes the full path and name of a DC file, reads it and uses the Trimble Coordinate service to convert it into a
    /// csib string
    /// </summary>
    string DCFileToCSIB(string filePath);

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a CSIB string
    /// </summary>
    string DCFileContentToCSIB(string filePath, byte[] fileContent);

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    CoordinateSystemResponse DCFileContentToCSD(string filePath, byte[] fileContent);

    /// <summary>
    /// Takes the CSIB string and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    CoordinateSystemResponse CSIBContentToCSD(string csib);
  }
}
