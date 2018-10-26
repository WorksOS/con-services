using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingFixture : IDisposable
  {
    private static object Lock = new object();

    public DILoggingFixture()
    {
      lock (Lock)
      {
        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Build()
          .Complete();
      }
    }

    public void Dispose()
    {
    } // Nothing needing doing 
  }
}
