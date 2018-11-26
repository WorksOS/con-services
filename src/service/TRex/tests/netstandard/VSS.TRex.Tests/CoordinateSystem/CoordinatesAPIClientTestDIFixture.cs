using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.Common.Utilities;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.HttpClients.Clients;
using VSS.TRex.HttpClients.RequestHandlers;
using VSS.TRrex.HttpClients.Abstractions;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinatesAPIClientTestDIFixture : IDisposable
  {
    private static readonly object Lock = new object();
    private static object DI;

    public CoordinatesAPIClientTestDIFixture()
    {
      lock (Lock)
      {
        DI = DI ?? DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())
          .Add(x => x.AddTransient<TPaaSAuthenticatedRequestHandler>()
          .AddHttpClient<ITPaaSClient, TPaaSClient>(client => client.BaseAddress = new Uri(EnvironmentHelper.GetEnvironmentVariable(TPaaSClient.TPAAS_AUTH_URL_ENV_KEY)))
          .ConfigurePrimaryHttpMessageHandler(() => new TPaaSApplicationCredentialsRequestHandler
            {
              TPaaSToken = EnvironmentHelper.GetEnvironmentVariable(TPaaSApplicationCredentialsRequestHandler.TPAAS_APP_TOKEN_ENV_KEY),
              InnerHandler = new HttpClientHandler()
            })
          .Services.AddHttpClient<CoordinatesServiceClient>(client => client.BaseAddress = new Uri(EnvironmentHelper.GetEnvironmentVariable(CoordinatesServiceClient.COORDINATE_SERVICE_URL_ENV_KEY)))
          .AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>())
          .Complete();
      }
    }

    public void Dispose()
    { }
  }
}
