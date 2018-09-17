using Apache.Ignite.Core;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.GridFabric.Grids
{
  public interface ITRexGridFactory
  {
    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in
    /// </summary>
    /// <param name="gridName"></param>
    /// <returns></returns>
    IIgnite Grid(string gridName);

    /// <summary>
    /// Creates an appropriate new Ignite grid reference depending on the TRex Grid passed in.
    /// If the grid reference has previously been requested it returned from a cached reference.
    /// </summary>
    /// <param name="mutability"></param>
    /// <returns></returns>
    IIgnite Grid(StorageMutability mutability);

    void ClearCache();
  }
}
