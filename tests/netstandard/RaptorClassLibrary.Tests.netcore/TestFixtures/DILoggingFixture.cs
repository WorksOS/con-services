using System;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.netcore.TestFixtures
{
  public class DILoggingFixture : IDisposable
  {
    private static object Lock = new object();

    public DILoggingFixture()
    {
      lock (Lock)
      {
          DIBuilder.New().AddLogging().Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }
}
