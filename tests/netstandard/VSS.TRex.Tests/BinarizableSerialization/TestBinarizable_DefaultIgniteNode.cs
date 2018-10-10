using Apache.Ignite.Core;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Provides a static singleton handle to a default Ignite node to allow access to Ignite.Binary() interfaces
  /// </summary>
  public static class TestBinarizable_DefaultIgniteNode
  {
    private static IIgnite _ignite;
    private static object lockObj = new object();

    public static IIgnite GetIgnite()
    {
      lock (lockObj)
      {
        if (_ignite != null)
          return _ignite;

        _ignite = Ignition.TryGetIgnite() ?? Ignition.Start();
        return _ignite;
      }
    }
  }
}
