using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Provides access to require coordinate conversion functions for TRex
  /// </summary>
  public static class Convert
  {
    private static ICoordinateConversion conversion = DI.DIContext.Obtain<ICoordinateConversion>();

    /// <summary>
    /// Provides a null conversion between the 2D coordinates in a WGS84 LL point and a XYZ NEE point.
    /// Only the 2D coordiantes are used, and directly copied from the LL point to the XY point, maintaining the
    /// X == Lonogitude and Y == Lattitude sense of the relative coordiantes
    /// </summary>
    /// <param name="WGSLL"></param>
    /// <returns></returns>
    public static XYZ NullWGSLLToXY(WGS84Point WGSLL) => new XYZ(WGSLL.Lon, WGSLL.Lat, Consts.NullDouble);

    public static XYZ[] NullWGSLLToXY(WGS84Point[] WGSLLs) => WGSLLs.Select(x => new XYZ(x.Lon, x.Lat)).ToArray();

    /// <summary>
    /// Provides a converion of WGS84 coorindates into the site calibration used by the project
    /// Right not, this is just a null conversion. 
    /// </summary>
    /// <param name="WGSLL"></param>
    /// <returns></returns>
    public static XYZ WGS84ToCalibration(string csib, WGS84Point WGSLL) => conversion.WGS84ToCalibration(csib, WGSLL);

    /// <summary>
    /// Provides a converion of WGS84 coorindates into the site calibration used by the project
    /// Right not, this is just a null conversion. 
    /// </summary>
    /// <param name="csib"></param>
    /// <param name="WGSLLs"></param>
    /// <returns></returns>
    public static XYZ[] WGS84ToCalibration(string csib, WGS84Point[] WGSLLs) => conversion.WGS84ToCalibration(csib, WGSLLs);

    /// <summary>
    /// Takes the full path and name of a DC file, reads it and uses the Trimble Coordiante service to convert it into a
    /// csib string
    /// </summary>
    /// <param name="DCFileName"></param>
    /// <returns></returns>
    public static string DCFileToCSIB(string DCFileName) => conversion.DCFileToCSIB(DCFileName);

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a CSIB string
    /// </summary>
    /// <param name="DCFileName"></param>
    /// <param name="fileContent"></param>
    /// <returns></returns>
    public static string DCFileContentToCSIB(string DCFileName, byte[] fileContent) => conversion.DCFileContentToCSIB(DCFileName, fileContent);
  }
}
