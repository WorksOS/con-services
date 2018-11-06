using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Trex.HTTPClients.Clients;
using VSS.Trex.HTTPClients.RequestHandlers;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.DI;
using VSS.TRex.HTTPClients.RequestHandlers;

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
          .Add(x => x.AddTransient<TPaaSAuthenticatedRequestHandler>())
          .Add(x => x.AddTransient<TPaaSApplicationCredentialsRequestHandler>())
          .AddHttpClient<TPaaSClient>(client => client.BaseAddress = new Uri("https://identity-stg.trimble.com/i/oauth2/token")
          ).AddHttpMessageHandler<TPaaSApplicationCredentialsRequestHandler>()
          .AddHttpClient<CoordinatesServiceClient>(client => client.BaseAddress = new Uri("https://api-stg.trimble.com/t/trimble.com/coordinates/1.0")
          ).AddHttpMessageHandler<TPaaSAuthenticatedRequestHandler>()
          .Complete();
      }
    }

    public void Dispose()
    { }
  }
}
