using System;
using System.Diagnostics;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.netcore.TestFixtures
{
  public class DILoggingFixture : IDisposable
  {
    private static object Lock = new object();
    private static object DI;

    public DILoggingFixture()
    {
      lock (Lock)
      {
        DI = DI ?? DIBuilder.New().AddLogging().Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }
}
