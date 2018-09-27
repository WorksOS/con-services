using System;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Factory responsible for creating concrete 'grid data' specific client sub grid leaf instances
  /// </summary>
  public class ClientLeafSubGridFactory : IClientLeafSubgridFactory
  {
    /// <summary>
    /// Simple array to hold client leaf subgrid type constructor map
    /// </summary>
    private Func<IClientLeafSubGrid>[] typeMap;

    /// <summary>
    /// Stores of cached client grids to reduce the object instantation and garbage collection overhead
    /// </summary>
//        private ConcurrentBag<IClientLeafSubGrid>[] ClientLeaves = Enumerable.Range(0, Enum.GetNames(typeof(GridDataType)).Length).Select(x => new ConcurrentBag<IClientLeafSubGrid>()).ToArray();
    private SimpleConcurrentBag<IClientLeafSubGrid>[] ClientLeaves = Enumerable.Range(0, Enum.GetNames(typeof(GridDataType)).Length).Select(x => new SimpleConcurrentBag<IClientLeafSubGrid>()).ToArray();

    public ClientLeafSubGridFactory()
    {
      typeMap = new Func<IClientLeafSubGrid>[Enum.GetNames(typeof(GridDataType)).Length];
    }

    /// <summary>
    /// Register a type implementing IClientLeafSubGrid against a grid data type for the factory to 
    /// create on demand
    /// </summary>
    /// <param name="gridDataType"></param>
    /// <param name="constructor"></param>
    public void RegisterClientLeafSubGridType(GridDataType gridDataType, Func<IClientLeafSubGrid> constructor)
    {
      if ((int) gridDataType > typeMap.Length)
        throw new ArgumentException("Unknown grid data type in RegisterClientLeafSubgridType", nameof(gridDataType));

      typeMap[(int) gridDataType] = constructor;
    }

    /// <summary>
    /// Construct a concrete instance of a subgrid implementing the IClientLeafSubGrid interface based
    /// on the role it should play according to the grid data type requested. All aspects of leaf ownership
    /// by a subgrid tree, parentage, level, cellsize, indexoriginoffset are delegated responsibilities
    /// of the caller or a derived factory class
    /// </summary>
    /// <param name="gridDataType"></param>
    /// <returns>An appropriate instance derived from ClientLeafSubgrid</returns>
    public IClientLeafSubGrid GetSubGrid(GridDataType gridDataType)
    {
      if (!ClientLeaves[(int) gridDataType].TryTake(out IClientLeafSubGrid result))
      {
        if (typeMap[(int) gridDataType] != null)
          result = typeMap[(int) gridDataType]();

        /*        
        result = (IClientLeafSubGrid) Activator.CreateInstance(
          typeMap[(int) gridDataType], // IClientLeafSubGrid type
          null, // Subgrid tree owner
          null, // Subgrid parent
          SubGridTreeConsts.SubGridTreeLevels, // Level, default to standard tree levels
          SubGridTreeConsts.DefaultCellSize, // Cell Size
          SubGridTreeConsts.DefaultIndexOriginOffset // IndexOfiginOffset, default to tree default value
        );
        */
      }

      result?.Clear();
      return result;
    }

    /// <summary>
    /// Return a client grid previous obtained from the factory so it may reuse it
    /// </summary>
    /// <param name="clientGrid"></param>
    public void ReturnClientSubGrid(ref IClientLeafSubGrid clientGrid)
    {
      if (clientGrid == null)
        return;

      // Make sure the type of the client grid being returned matches it's advertised grid type
      // if (!typeMap[(int)clientGrid.GridDataType].Equals(clientGrid.GetType()))
      // {
      //    Debug.Assert(false, "Type of client grid being returned does not match advertised grid data type.");
      // }

      ClientLeaves[(int) clientGrid.GridDataType].Add(clientGrid);
      clientGrid = null;
    }

    /// <summary>
    /// Return an array of client grids (of the same type) previously obtained from the factory so it may reuse them
    /// </summary>
    /// <param name="clientGrids"></param>
    /// <param name="count"></param>
    public void ReturnClientSubGrids(IClientLeafSubGrid[] clientGrids, int count)
    {
      if (count < 0 || count > clientGrids.Length)
        throw new ArgumentException("Invalid count of subgrids to return", nameof(count));

      for (int i = 0; i < count; i++)
        ReturnClientSubGrid(ref clientGrids[i]);
    }

    /// <summary>
    /// Return an array of client grids (of the same type) previously obtained from the factory so it may reuse them
    /// </summary>
    /// <param name="clientGrids"></param>
    /// <param name="count"></param>
    public void ReturnClientSubGrids(IClientLeafSubGrid[][] clientGrids, int count)
    {
      if (count < 0 || count > clientGrids.Length)
        throw new ArgumentException("Invalid count of subgrids to return", nameof(count));

      for (int i = 0; i < count; i++)
      {
        for (int j = 0; j < clientGrids[i]?.Length; j++)
          ReturnClientSubGrid(ref clientGrids[i][j]);
      }
    }
  }
}
