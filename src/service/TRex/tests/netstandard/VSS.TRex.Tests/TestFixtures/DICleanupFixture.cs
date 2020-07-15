using System;
using VSS.TRex.DI;

namespace VSS.TRex.Tests.TestFixtures
{
  /// <summary>
  /// The sole purpose of this fixture is to ensure that the DIContext is killed at the
  /// end of the tests in a test class.
  /// </summary>
  public class DICleanupFixture : IDisposable
  {
    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
