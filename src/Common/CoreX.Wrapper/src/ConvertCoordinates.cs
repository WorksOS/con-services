using System.Linq;
using CoreX.Wrapper.Extensions;
using CoreX.Wrapper.Models;
using CoreX.Wrapper.Types;

namespace CoreX.Wrapper
{
  /// <summary>
  /// Implements a set of capabilities for coordinate conversion between WGS and grid contexts, and
  /// conversion of coordinate system files into CSIB (Coordinate System Information Block) strings.
  /// </summary>
  /// <remarks>
  /// While these methods can be called directly, it's recommended to utilize the static ConvertCoordinates helper.
  /// </remarks>
  public class ConvertCoordinates : IConvertCoordinates
  {
    private readonly CoreX _coreX;

    public ConvertCoordinates(CoreX coreX)
    {
      _coreX = coreX;
    }

    /// <inheritdoc />
    public XYZ NullWGSLLToXY(WGS84Point wgsPoint) => new XYZ(wgsPoint.Lon, wgsPoint.Lat);

    /// <inheritdoc />
    public XYZ[] NullWGSLLToXY(WGS84Point[] wgsPoints) => wgsPoints.Select(x => new XYZ(x.Lon, x.Lat)).ToArray();

    /// <inheritdoc />
    public NEE LLHToNEE(string csib, LLH coordinates, InputAs inputAs)
    {
      if (inputAs == InputAs.Degrees)
      {
        coordinates.Latitude = coordinates.Latitude.DegreesToRadians();
        coordinates.Longitude = coordinates.Longitude.DegreesToRadians();
      }

      return _coreX
        .SetCsibFromBase64String(csib)
        .TransformLLHToNEE(coordinates, fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);
    }

    /// <inheritdoc />
    public NEE[] LLHToNEE(string csib, LLH[] coordinates, InputAs inputAs)
    {
      if (inputAs == InputAs.Degrees)
      {
        for (var i = 0; i < coordinates.Length; i++)
        {
          var llh = coordinates[i];
          llh.Latitude = llh.Latitude.DegreesToRadians();
          llh.Longitude = llh.Longitude.DegreesToRadians();
        }
      }

      return _coreX
        .SetCsibFromBase64String(csib)
        .TransformLLHToNEE(coordinates, fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);
    }

    /// <inheritdoc />
    public XYZ LLHToNEE(string csib, XYZ coordinates, InputAs inputAs)
    {
      if (inputAs == InputAs.Degrees)
      {
        coordinates.X = coordinates.X.DegreesToRadians();
        coordinates.Y = coordinates.Y.DegreesToRadians();
      }

      var neeCoords = _coreX
        .SetCsibFromBase64String(csib)
        .TransformLLHToNEE(coordinates.ToLLH(), fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      return new XYZ
      {
        X = neeCoords.East,
        Y = neeCoords.North,
        Z = neeCoords.Elevation
      };
    }

    /// <inheritdoc/>
    public XYZ[] LLHToNEE(string csib, XYZ[] coordinates, InputAs inputAs)
    {
      if (inputAs == InputAs.Degrees)
      {
        for (var i = 0; i < coordinates.Length; i++)
        {
          var xyz = coordinates[i];
          xyz.X = xyz.X.DegreesToRadians();
          xyz.Y = xyz.Y.DegreesToRadians();

          coordinates[i] = xyz;
        }
      }

      var neeCoords = _coreX
        .SetCsibFromBase64String(csib)
        .TransformLLHToNEE(coordinates.ToLLH(), fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      var responseArray = new XYZ[neeCoords.Length];

      for (var i = 0; i < neeCoords.Length; i++)
      {
        var nee = neeCoords[i];

        responseArray[i] = new XYZ
        {
          X = nee.East,
          Y = nee.North,
          Z = nee.Elevation
        };
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public XYZ NEEToLLH(string csib, XYZ coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var llhCoords = _coreX
        .SetCsibFromBase64String(csib)
        .TransformNEEToLLH(coordinates.ToNEE(), fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var inDegrees = returnAs == ReturnAs.Degrees;

      return new XYZ
      {
        Y = inDegrees ? llhCoords.Latitude.RadiansToDegrees() : llhCoords.Latitude,
        X = inDegrees ? llhCoords.Longitude.RadiansToDegrees() : llhCoords.Longitude,
        Z = llhCoords.Height
      };
    }

    /// <inheritdoc/>
    public XYZ[] NEEToLLH(string csib, XYZ[] coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var llhCoords = _coreX
        .SetCsibFromBase64String(csib)
        .TransformNEEToLLH(coordinates.ToNEE(), fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var responseArray = new XYZ[llhCoords.Length];
      var inDegrees = returnAs == ReturnAs.Degrees;

      for (var i = 0; i < llhCoords.Length; i++)
      {
        var llh = llhCoords[i];

        responseArray[i] = new XYZ
        {
          Y = inDegrees ? llh.Latitude.RadiansToDegrees() : llh.Latitude,
          X = inDegrees ? llh.Longitude.RadiansToDegrees() : llh.Longitude,
          Z = llh.Height
        };
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public LLH NEEToLLH(string csib, NEE coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var llhCoords = _coreX
        .SetCsibFromBase64String(csib)
        .TransformNEEToLLH(coordinates, fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var inDegrees = returnAs == ReturnAs.Degrees;

      return new LLH
      {
        Longitude = inDegrees ? llhCoords.Longitude.RadiansToDegrees() : llhCoords.Longitude,
        Latitude = inDegrees ? llhCoords.Latitude.RadiansToDegrees() : llhCoords.Latitude,
        Height = llhCoords.Height
      };
    }

    /// <inheritdoc/>
    public LLH[] NEEToLLH(string csib, NEE[] coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var llhCoords = _coreX
                      .SetCsibFromBase64String(csib)
                      .TransformNEEToLLH(coordinates, fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var responseArray = new LLH[llhCoords.Length];
      var inDegrees = returnAs == ReturnAs.Degrees;

      for (var i = 0; i < llhCoords.Length; i++)
      {
        var llh = llhCoords[i];

        responseArray[i] = new LLH
        {
          Longitude = inDegrees ? llh.Longitude.RadiansToDegrees() : llh.Longitude,
          Latitude = inDegrees ? llh.Latitude.RadiansToDegrees() : llh.Latitude,
          Height = llh.Height
        };
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public XYZ WGS84ToCalibration(string id, WGS84Point wgs84Point)
    {
      var nee = _coreX
        .SetCsibFromBase64String(id)
        .TransformLLHToNEE(new LLH
        {
          Latitude = wgs84Point.Lat.DegreesToRadians(),
          Longitude = wgs84Point.Lon.DegreesToRadians(),
          Height = wgs84Point.Height
        },
        fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      return new XYZ
      {
        X = nee.East,
        Y = nee.North,
        Z = nee.Elevation
      };
    }

    /// <inheritdoc/>
    public XYZ[] WGS84ToCalibration(string id, WGS84Point[] wgs84Points)
    {
      var neeCoords = _coreX
                .SetCsibFromBase64String(id)
                .TransformLLHToNEE(wgs84Points.ToLLH(), fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      var responseArray = new XYZ[neeCoords.Length];

      for (var i = 0; i < neeCoords.Length; i++)
      {
        var llh = neeCoords[i];

        responseArray[i] = new XYZ
        {
          X = llh.East,
          Y = llh.North,
          Z = llh.Elevation
        };
      }

      return responseArray;
    }

    /// <inheritdoc cref="CoreX.GetCSIBFromDCFile"/>
    public string DCFileToCSIB(string filePath) => _coreX.GetCSIBFromDCFile(filePath);

    /// <inheritdoc/>
    public string GetCSIBFromDCFileContent(string fileContent) => _coreX.GetCSIBFromDCFileContent(fileContent);
  }
}
