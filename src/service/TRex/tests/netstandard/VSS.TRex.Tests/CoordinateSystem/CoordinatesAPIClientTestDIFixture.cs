using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.CoordinateSystem
{
  public class CoordinatesAPIClientTestDIFixture : DILoggingFixture, IDisposable
  {
    public CoordinatesAPIClientTestDIFixture()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<ITPaasProxy, TPaasProxy>())
        .Complete();
    }

    public new void Dispose()
    {
      base.Dispose();
      DIBuilder.Eject();
    }
  }
}
