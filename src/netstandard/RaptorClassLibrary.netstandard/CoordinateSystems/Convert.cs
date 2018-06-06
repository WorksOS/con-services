using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.TRex.Common;
using VSS.TRex.Geometry;
using VSS.TRex.Types;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Provides access to require coordinate conversion functions for TRex
  /// </summary>
  public static class Convert
  {
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
    /// Right not, this is just a null conversion. Todo: Hook this into the Trimble Coordiantes service
    /// </summary>
    /// <param name="WGSLL"></param>
    /// <returns></returns>
    public static XYZ WGS84ToCalibration(Guid ProjectID, WGS84Point WGSLL) => NullWGSLLToXY(WGSLL);

    public static XYZ[] WGS84ToCalibration(Guid ProjectID, WGS84Point[] WGSLLs) => NullWGSLLToXY(WGSLLs); 
  }
}
