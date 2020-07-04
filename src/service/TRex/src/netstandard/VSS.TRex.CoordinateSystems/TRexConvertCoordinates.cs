using System;
using System.Net.Http;
using System.Threading.Tasks;
using CoreX.Models;
using Microsoft.Extensions.DependencyInjection;
using VSS.Common.Abstractions.Configuration;
using VSS.Tpaas.Client.Abstractions;
using VSS.Tpaas.Client.Clients;
using VSS.Tpaas.Client.RequestHandlers;
using VSS.TRex.DI;
using VSS.TRex.HttpClients;

namespace VSS.TRex.CoordinateSystems
{
  /// <summary>
  /// Implements a set of capabilities for coordinate conversion between WGS and grid contexts, and
  /// conversion of coordinate system files into CSIB (Coordinate System Information Block) strings.
  /// </summary>
  /// <remarks>
  /// While these methods can be called directly, it's recommended to utilize the static ConvertCoordinates helper.
  /// </remarks>
  public class TRexConvertCoordinates : ITRexConvertCoordinates
  {
    private static readonly object lockObject = new object();

    public TRexConvertCoordinates()
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
    /// Takes the content of a DC file as a byte array and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public Task<CoordinateSystemResponse> DCFileContentToCSD(string filePath, byte[] fileContent) => serviceClient.ImportCSDFromDCContentAsync(filePath, fileContent);

    /// <summary>
    /// Takes the CSIB string and uses the Trimble Coordinates Service to convert
    /// it into a coordinate system definition response object.
    /// </summary>
    public Task<CoordinateSystemResponse> CSIBContentToCSD(string csib) => serviceClient.ImportCSDFromCSIBAsync(csib);
  }
}
