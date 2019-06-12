using Apache.Ignite.Core;
using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Grids
{
  public class TRexGridFactory : ITRexGridFactory
  {
    /// <summary>
    /// An array of ignite reference whose size matches the enumeration of ignite grids
    /// </summary>
    private IIgnite[] igniteGrids;

    public TRexGridFactory()
    {
      CreateCache();
    }

    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in.
    /// If the grid reference has previously been requested it returned from a cached reference.
    /// </summary>
    /// <param name="mutability"></param>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public IIgnite Grid(StorageMutability mutability, IgniteConfiguration cfg = null)
    {
      return igniteGrids[(int) mutability] ?? (igniteGrids[(int) mutability] = IgniteGridFactory(TRexGrids.GridName(mutability), cfg));
    }

    private void CreateCache()
    {
      igniteGrids = new IIgnite[Enum.GetValues(typeof(StorageMutability)).Cast<int>().Max() + 1];
    }

    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public IIgnite Grid(string gridName, IgniteConfiguration cfg = null)
    {
      if (true == gridName?.Equals(TRexGrids.MutableGridName()))
        return Grid(StorageMutability.Mutable, cfg);

      if (true == gridName?.Equals(TRexGrids.ImmutableGridName()))
        return Grid(StorageMutability.Immutable, cfg);

      throw new TRexException($"{gridName} is an unknown grid to create a reference for.");
    }

    /// <summary>
    /// The default factory for obtaining or creating ignite nodes. This method is injected into the
    /// DI context as the Func(string, IIgnite) factory delegate obtained from the DIContext in Grid()
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="cfg"></param>
    /// <returns></returns>
    public static IIgnite IgniteGridFactory(string gridName, IgniteConfiguration cfg = null)
    {
      return DIContext.Obtain<Func<string, IgniteConfiguration, IIgnite>>()(gridName, cfg);
    }

    /// <summary>
    /// If the calling context is directly using an IServiceCollection then obtain the DIBuilder based on it before adding...
    /// </summary>
    /// <param name="services"></param>
    public static void AddGridFactoriesToDI(IServiceCollection services)
    {
      DIBuilder.Continue(services).Add(x => AddDIEntries());
    }

    private static void AddDIEntries()
    {
      DIBuilder.Continue()
        .Add(x => x.AddSingleton<IActivatePersistentGridServer>(new ActivatePersistentGridServer()))
        .Add(x => x.AddSingleton<Func<string, IgniteConfiguration, IIgnite>>(factory => (gridName, cfg) => Ignition.TryGetIgnite(gridName) ?? (cfg == null ? null : Ignition.Start(cfg))))
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()));
    }

    private void StopGrid(StorageMutability mutability)
    {
      if (igniteGrids[(int)mutability] != null)
      {
        Ignition.Stop(igniteGrids[(int)mutability].Name, false);
        igniteGrids[(int)mutability] = null;
      }
    }

    public void StopGrids()
    {
      StopGrid(StorageMutability.Immutable);
      StopGrid(StorageMutability.Mutable);
    }
  }
}
