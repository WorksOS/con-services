using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Clients;
using VSS.Tpaas.Client.RequestHandlers;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.HttpClients;
using VSS.TRex.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;

namespace VSS.TRex.CoordinateSystems
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
    private static readonly object lockObject = new object();
    
    public ConvertCoordinates()
    {
      var configurationStore = DIContext.Obtain<IConfigurationStore>();

      lock (lockObject)
      {
        DIBuilder
          .Continue()
          .Add(x => x.AddTransient<TRexTPaaSAuthenticatedRequestHandler>()
            .AddHttpClient<ITPaaSClient, TPaaSClient>(client => client.BaseAddress = new Uri(configurationStore.GetValueString(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY)))
            .ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
            {
              TPaaSToken = configurationStore.GetValueString(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
              InnerHandler = new HttpClientHandler()
            })
            .Services.AddHttpClient<CoordinatesServiceClient>(client => client.BaseAddress = new Uri(configurationStore.GetValueString(CoordinatesServiceClient.COORDINATE_SERVICE_URL_ENV_KEY)))
            .AddHttpMessageHandler<TRexTPaaSAuthenticatedRequestHandler>())
          .Complete();
      }

      serviceClient = DIContext.Obtain<CoordinatesServiceClient>();
    }

    private readonly CoordinatesServiceClient serviceClient;

    /// <summary>
    /// Provides a null conversion between the 2D coordinates in a WGS84 LL point and a XYZ NEE point.
    /// Only the 2D coordinates are used, and directly copied from the LL point to the XY point, maintaining the
    /// X == Longitude and Y == Latitude sense of the relative coordinates
    /// </summary>
    public XYZ NullWGSLLToXY(WGS84Point WGSLL) => new XYZ(WGSLL.Lon, WGSLL.Lat, Consts.NullDouble);

    /// <summary>
    /// Takes an array of <see cref="WGS84Point"/> and uses the Coordinate Service to convert it into <see cref="XYZ"/> data.
    /// </summary>
    public XYZ[] NullWGSLLToXY(WGS84Point[] WGSLLs) => WGSLLs.Select(x => new XYZ(x.Lon, x.Lat)).ToArray();

    /// <summary>
    /// Takes a <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public async Task<NEE> LLHToNEE(string id, LLH LLH, bool convertToRadians = true)
    {
      if (convertToRadians)
      {
        LLH.Longitude = MathUtilities.RadiansToDegrees(LLH.Longitude);
        LLH.Latitude = MathUtilities.RadiansToDegrees(LLH.Latitude);
      }

      return await serviceClient.GetNEEFromLLHAsync(id, LLH);
    }

    /// <summary>
    /// Takes an array of <see cref="LLH"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, NEE[] NEECoordinates)> LLHToNEE(string id, LLH[] LLH, bool convertToRadians = true)
    {
      return await serviceClient.GetNEEFromLLHAsync(id, LLH.ToRequestArray(convertToRadians));
    }

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="LLH"/> data into <see cref="NEE"/> data.
    /// </summary>
    public async Task<XYZ> LLHToNEE(string id, XYZ coordinates, bool convertToRadians = true)
    {
      var result = await serviceClient.GetNEEFromLLHAsync(id, coordinates.ToLLH(convertToRadians));

      return new XYZ
      {
        X = result.East,
        Y = result.North,
        Z = result.Elevation
      };
    }

    /// <summary>
    /// Converts <see cref="XYZ"/> coordinates holding <see cref="NEE"/> data into <see cref="LLH"/> data.
    /// </summary>
    public async Task<XYZ> NEEToLLH(string id, XYZ coordinates)
    {
      var result = await serviceClient.GetLLHFromNEEAsync(id, coordinates.ToNEE());

      return new XYZ
      {
        X = result.Longitude,
        Y = result.Latitude,
        Z = result.Height
      };
    }

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="NEE"/> data.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, XYZ[] NEECoordinates)> LLHToNEE(string id, XYZ[] coordinates, bool convertToRadians = true)
    {
      var result = await serviceClient.GetNEEFromLLHAsync(id, coordinates.ToLLHRequestArray(convertToRadians));
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        return (result.ErrorCode, null);
      }

      var NEECoords = new XYZ[result.NEECoordinates.Length];

      for (var i = 0; i < result.NEECoordinates.Length; i++)
      {
        NEECoords[i].X = result.NEECoordinates[i].East;
        NEECoords[i].Y = result.NEECoordinates[i].North;
        NEECoords[i].Z = result.NEECoordinates[i].Elevation;
      }

      return (RequestErrorStatus.OK, NEECoords);
    }

    /// <summary>
    /// Takes an array of <see cref="XYZ"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, XYZ[] LLHCoordinates)> NEEToLLH(string id, XYZ[] coordinates)
    {
      var result = await serviceClient.GetLLHFromNEEAsync(id, coordinates.ToNEERequestArray());
      if (result.ErrorCode != RequestErrorStatus.OK)
      {
        var log = DIContext.Obtain<ILoggerFactory>().CreateLogger(nameof(NEEToLLH));
        log.LogError($"{nameof(NEEToLLH)} Failed to convert Coordinates NEEToLLH. Error: {result.ErrorCode} Coords: {JsonConvert.SerializeObject(result.LLHCoordinates)}");

        return (result.ErrorCode, null);
      }

      var LLHCoords = new XYZ[result.LLHCoordinates.Length];
      for (var i = 0; i < result.LLHCoordinates.Length; i++)
      {
        LLHCoords[i].X = result.LLHCoordinates[i].Longitude;
        LLHCoords[i].Y = result.LLHCoordinates[i].Latitude;
        LLHCoords[i].Z = result.LLHCoordinates[i].Height;
      }

      return (RequestErrorStatus.OK, LLHCoords);
    }

    /// <summary>
    /// Takes a <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public async Task<LLH> NEEToLLH(string id, NEE NEE) => await serviceClient.GetLLHFromNEEAsync(id, NEE);

    /// <summary>
    /// Takes an array of <see cref="NEE"/> and uses the Coordinate Service to convert it into <see cref="LLH"/> data.
    /// </summary>
    public async Task<(RequestErrorStatus ErrorCode, LLH[] LLHCoordinates)> NEEToLLH(string id, NEE[] NEE) => await serviceClient.GetLLHFromNEEAsync(id, NEE.ToRequestArray());

    /// <summary>
    /// Uses the Coordinate Service to convert WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    public async Task<XYZ> WGS84ToCalibration(string id, WGS84Point wgs84Point, bool convertToRadians = true)
    {
      var nee = await serviceClient.GetNEEFromLLHAsync(id, new LLH
      {
        Latitude = convertToRadians ? MathUtilities.RadiansToDegrees(wgs84Point.Lat) : wgs84Point.Lat,
        Longitude = convertToRadians ? MathUtilities.RadiansToDegrees(wgs84Point.Lon) : wgs84Point.Lon,
        Height = wgs84Point.Height
      });

      return new XYZ
      {
        X = nee.North,
        Y = nee.East,
        Z = nee.Elevation
      };
    }

    /// <summary>
    /// Uses the Coordinate Service to convert an array of WGS84 coordinates into the site calibration used by the project.
    /// </summary>
    public async Task<XYZ[]> WGS84ToCalibration(string id, WGS84Point[] wgs84Points, bool convertToRadians = true)
    {
      (var errorCode, NEE[] neeCoordinates) = await serviceClient.GetNEEFromLLHAsync(id, wgs84Points.ToRequestArray(convertToRadians));

      if (errorCode != RequestErrorStatus.OK)
      {
        return null;
      }

      var xyzArray = new XYZ[neeCoordinates.Length];

      for (var i = 0; i < wgs84Points.Length; i++)
      {
        xyzArray[i] = new XYZ
        {
          X = neeCoordinates[i].East,
          Y = neeCoordinates[i].North,
          Z = neeCoordinates[i].Elevation
        };
      }

      return xyzArray;
    }

    /// <summary>
    /// Takes the full path and name of a DC file, reads it and uses the Trimble Coordinate service to convert it into a
    /// csib string
    /// </summary>
    public async Task<string> DCFileToCSIB(string filePath) => await serviceClient.ImportFromDCAsync(filePath);

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a CSIB string
    /// </summary>
    public async Task<string> DCFileContentToCSIB(string filePath, byte[] fileContent) => await serviceClient.ImportFromDCContentAsync(filePath, fileContent);

    /// <summary>
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public async Task<CoordinateSystemResponse> DCFileContentToCSD(string filePath, byte[] fileContent) => await serviceClient.ImportCSDFromDCContentAsync(filePath, fileContent);

    /// <summary>
    /// Takes the CSIB string and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public async Task<CoordinateSystemResponse> CSIBContentToCSD(string csib) => await serviceClient.ImportCSDFromCSIBAsync(csib);
  }
}
