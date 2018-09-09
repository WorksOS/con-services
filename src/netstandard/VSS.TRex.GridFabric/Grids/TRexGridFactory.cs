using Apache.Ignite.Core;
using System;
using System.Linq;
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
    /// <returns></returns>
    public IIgnite Grid(StorageMutability mutability)
    {
      return igniteGrids[(int) mutability] ?? (igniteGrids[(int) mutability] = Ignition.TryGetIgnite(TRexGrids.GridName(mutability)));
    }

    private void CreateCache()
    {
      igniteGrids = new IIgnite[Enum.GetValues(typeof(StorageMutability)).Cast<int>().Max(x => x) + 1];
    }

    public void ClearCache()
    {
      CreateCache();
    }
    
    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in
    /// </summary>
    /// <param name="gridName"></param>
    /// <returns></returns>
    public IIgnite Grid(string gridName)
    {
      if (gridName.Equals(TRexGrids.MutableGridName()))
      {
        return igniteGrids[(int)StorageMutability.Mutable] ?? (igniteGrids[(int)StorageMutability.Mutable] = Ignition.TryGetIgnite(gridName));
      }

      if (gridName.Equals(TRexGrids.ImmutableGridName()))
      {
        return igniteGrids[(int)StorageMutability.Immutable] ?? (igniteGrids[(int)StorageMutability.Immutable] = Ignition.TryGetIgnite(gridName));
      }

      throw new ArgumentException($"{gridName} is an unknown grid to create a reference for.");
    }
  }
}
