using System;
using CoreX.Extensions;
using CoreX.Interfaces;
using CoreX.Types;
using CoreXModels;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Serilog.Extensions;

namespace CoreX.Wrapper
{
  /// <summary>
  /// Implements a set of capabilities for coordinate conversion between WGS and grid contexts, and
  /// conversion of coordinate system files into CSIB (Coordinate System Information Block) strings.
  /// </summary>
  /// <remarks>
  /// While these methods can be called directly, it's recommended to utilize the static ConvertCoordinates helper.
  /// </remarks>
  public class CoreXWrapper : ICoreXWrapper, IDisposable
  {
    private readonly CoreX _coreX;
    private readonly ILogger _log;

    public CoreX GetCoreX() => _coreX;

    public CoreXWrapper(ILoggerFactory loggerFactory, IConfigurationStore configStore)
    {
      _log = loggerFactory.CreateLogger<CoreXWrapper>();
      _coreX = new CoreX(loggerFactory, configStore);
    }

    public CoreXWrapper()
    { }

    public string GeodeticDatabasePath => _coreX.GeodeticDatabasePath;

    /// <inheritdoc />
    public XYZ NullWGSLLToXY(WGS84Point wgsPoint) => new XYZ(wgsPoint.Lon, wgsPoint.Lat);

    /// <inheritdoc />
    public NEE LLHToNEE(string csib, LLH coordinates, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, LLH: {coordinates}, InputAs: {inputAs}");
      }

      if (inputAs == InputAs.Degrees)
      {
        coordinates.Latitude = coordinates.Latitude.DegreesToRadians();
        coordinates.Longitude = coordinates.Longitude.DegreesToRadians();
      }

      var result = _coreX.TransformLLHToNEE(
        csib,
        coordinates,
        fromType: CoordinateTypes.ReferenceGlobalLLH,
        toType: CoordinateTypes.OrientatedNEE);

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, Returning NEE: {result}");
      }

      return result;
    }

    /// <inheritdoc />
    public NEE[] LLHToNEE(string csib, LLH[] coordinates, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, LLH[]: {string.Concat(coordinates)}, InputAs: {inputAs}, CSIB: {csib}");
      }

      if (inputAs == InputAs.Degrees)
      {
        for (var i = 0; i < coordinates.Length; i++)
        {
          var llh = coordinates[i];
          llh.Latitude = llh.Latitude.DegreesToRadians();
          llh.Longitude = llh.Longitude.DegreesToRadians();
        }
      }

      var result = _coreX.TransformLLHToNEE(
        csib,
        coordinates,
        fromType: CoordinateTypes.ReferenceGlobalLLH,
        toType: CoordinateTypes.OrientatedNEE);

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, Returning NEE[]: {string.Concat(result)}");
      }

      return result;
    }

    /// <inheritdoc />
    public XYZ LLHToNEE(string csib, XYZ coordinates, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, XYZ: {coordinates}, InputAs: {inputAs}, CSIB: {csib}");
      }

      if (inputAs == InputAs.Degrees)
      {
        coordinates.X = coordinates.X.DegreesToRadians();
        coordinates.Y = coordinates.Y.DegreesToRadians();
      }

      var neeCoords = _coreX
        .TransformLLHToNEE(csib, coordinates.ToLLH(), fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      var result = new XYZ
      {
        X = neeCoords.East,
        Y = neeCoords.North,
        Z = neeCoords.Elevation
      };

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, Returning XYZ: {result}");
      }

      return result;
    }

    /// <inheritdoc/>
    public XYZ[] LLHToNEE(string csib, XYZ[] coordinates, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, XYZ[]: {string.Concat(coordinates)}, InputAs: {inputAs}, CSIB: {csib}");
      }

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
        .TransformLLHToNEE(csib, coordinates.ToLLH(), fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

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

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(LLHToNEE)}: CoreXRequestID: {requestId}, Returning XYZ[]: {string.Concat(responseArray)}");
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public XYZ NEEToLLH(string csib, XYZ coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, XYZ: {coordinates}, ReturnAs: {returnAs}, CSIB: {csib}");
      }

      var llhCoords = _coreX
        .TransformNEEToLLH(csib, coordinates.ToNEE(), fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var inDegrees = returnAs == ReturnAs.Degrees;

      var result = new XYZ
      {
        Y = inDegrees ? llhCoords.Latitude.RadiansToDegrees() : llhCoords.Latitude,
        X = inDegrees ? llhCoords.Longitude.RadiansToDegrees() : llhCoords.Longitude,
        Z = llhCoords.Height
      };

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, Returning XYZ {result}");
      }

      return result;
    }

    /// <inheritdoc/>
    public XYZ[] NEEToLLH(string csib, XYZ[] coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, XYZ[]: {string.Concat(coordinates)}, ReturnAs: {returnAs}, CSIB: {csib}");
      }

      var llhCoords = _coreX
        .TransformNEEToLLH(csib, coordinates.ToNEE(), fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

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

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, Returning XYZ[]: {string.Concat(responseArray)}");
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public LLH NEEToLLH(string csib, NEE coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, NEE: {coordinates}, ReturnAs: {returnAs}, CSIB: {csib}");
      }

      var llhCoords = _coreX
        .TransformNEEToLLH(csib, coordinates, fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

      var inDegrees = returnAs == ReturnAs.Degrees;

      var result = new LLH
      {
        Longitude = inDegrees ? llhCoords.Longitude.RadiansToDegrees() : llhCoords.Longitude,
        Latitude = inDegrees ? llhCoords.Latitude.RadiansToDegrees() : llhCoords.Latitude,
        Height = llhCoords.Height
      };

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, Returning LLH: {result}");
      }

      return result;
    }

    /// <inheritdoc/>
    public LLH[] NEEToLLH(string csib, NEE[] coordinates, ReturnAs returnAs = ReturnAs.Radians)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, NEE[]: {string.Concat(coordinates)}, ReturnAs: {returnAs}, CSIB: {csib}");
      }

      var llhCoords = _coreX
        .TransformNEEToLLH(csib, coordinates, fromType: CoordinateTypes.OrientatedNEE, toType: CoordinateTypes.ReferenceGlobalLLH);

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

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(NEEToLLH)}: CoreXRequestID: {requestId}, Returning LLH[]: {string.Concat(responseArray)}");
      }

      return responseArray;
    }

    /// <inheritdoc/>
    public XYZ WGS84ToCalibration(string csib, WGS84Point wgs84Point, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(WGS84ToCalibration)}: CoreXRequestID: {requestId}, wgs84Point: {wgs84Point}, InputAs: {inputAs}, CSIB: {csib}");
      }

      if (inputAs == InputAs.Degrees)
      {
        wgs84Point.Lat = wgs84Point.Lat.DegreesToRadians();
        wgs84Point.Lon = wgs84Point.Lon.DegreesToRadians();
      }

      var nee = _coreX
        .TransformLLHToNEE(csib, new LLH
        {
          Latitude = wgs84Point.Lat,
          Longitude = wgs84Point.Lon,
          Height = wgs84Point.Height
        },
        fromType: CoordinateTypes.ReferenceGlobalLLH, toType: CoordinateTypes.OrientatedNEE);

      var result = new XYZ
      {
        X = nee.East,
        Y = nee.North,
        Z = nee.Elevation
      };

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(WGS84ToCalibration)}: CoreXRequestID: {requestId}, Returning XYZ: {result}");
      }

      return result;
    }

    /// <inheritdoc/>
    public XYZ[] WGS84ToCalibration(string csib, WGS84Point[] wgs84Points, InputAs inputAs)
    {
      var requestId = Guid.NewGuid();

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(WGS84ToCalibration)}: CoreXRequestID: {requestId}, wgs84Points[]: {string.Concat<WGS84Point>(wgs84Points)}, InputAs: {inputAs} CSIB: {csib}");
      }

      var neeCoords = _coreX
        .TransformLLHToNEE(
          csib,
          wgs84Points.ToLLH(inputAs),
          fromType: CoordinateTypes.ReferenceGlobalLLH,
          toType: CoordinateTypes.OrientatedNEE);

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

      if (_log.IsTraceEnabled())
      {
        _log.LogTrace($"{nameof(WGS84ToCalibration)}: CoreXRequestID: {requestId}, Returning XYZ[]: {string.Concat(responseArray)}");
      }

      return responseArray;
    }

    /// <inheritdoc cref="CoreX.GetCSIBFromDCFile"/>
    public string DCFileToCSIB(string filePath) => _coreX.GetCSIBFromDCFile(filePath);

    /// <inheritdoc/>
    public string GetCSIBFromDCFileContent(string fileContent) => _coreX.GetCSIBStringFromDCFileContent(fileContent);

    /// <inheritdoc/>
    public string GetCSIBFromCSDSelection(string zoneGroupNameString, string zoneNameString) => _coreX.GetCSIBFromCSDSelection(zoneGroupNameString, zoneNameString);

    /// <inheritdoc/>
    public Datum[] GetDatums() => _coreX.GetDatums();

    /// <inheritdoc/>
    public CoordinateSystem GetCSDFromCSIB(string csib) => _coreX.CSDResolver.GetCSDFromCSIB(csib);

    /// <inheritdoc/>
    public CoordinateSystem GetCSDFromDCFileContent(string fileContent) => _coreX.CSDResolver.GetCSDFromDCFileContent(fileContent);

    private bool _disposed = false;

    public void Dispose() => Dispose(true);

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
      {
        return;
      }

      if (disposing)
      {
        _coreX?.Dispose();
      }

      _disposed = true;
    }
  }
}
