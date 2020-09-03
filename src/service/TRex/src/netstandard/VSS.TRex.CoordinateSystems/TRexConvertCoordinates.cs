using System;
using System.Net.Http;
using System.Threading.Tasks;
using CoreXModels;
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
  public class TRexConvertCoordinates : ITRexConvertCoordinates
  {
    private readonly CoordinatesServiceClient _serviceClient;
    private static readonly object _lockObject = new object();

    public TRexConvertCoordinates()
    {
      var configurationStore = DIContext.Obtain<IConfigurationStore>();

      lock (_lockObject)
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

      _serviceClient = DIContext.Obtain<CoordinatesServiceClient>();
    }

    /// <inheritdoc/>
    public Task<CoordinateSystemResponse> DCFileContentToCSD(string filePath, byte[] fileContent) => _serviceClient.ImportCSDFromDCContentAsync(filePath, fileContent);

    /// <inheritdoc/>
    public Task<string> CSIBContentToCoordinateServiceId(string csib) => _serviceClient.ImportCoordinateServiceIdFromCSIBAsync(csib);

    /// <inheritdoc/>
    public Task<CoordinateSystemResponse> CoordinateSystemIdToCSD(string csib) => _serviceClient.ImportCSDFromCoordinateySystemId(csib);
  }
}
