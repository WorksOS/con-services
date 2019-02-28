using System;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  /// <summary>
  /// Provides a static singleton handle to a default Ignite node to allow access to Ignite.Binary() interfaces
  /// </summary>
  public static class TestBinarizable_DefaultIgniteNode
  {
    private static IIgnite _ignite;
    private static readonly object lockObj = new object();

    public static IIgnite GetIgnite(bool force = false)
    {
      if (!force)
        throw new InvalidOperationException("Running Ignite nodes in unit tests is undesirable if possible...");

      lock (lockObj)
      {
        if (_ignite != null)
          return _ignite;

        _ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(TRexGrids.ImmutableGridName(), new IgniteConfiguration());
        return _ignite;
      }
    }
  }
}
