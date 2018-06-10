using System;
using System.Linq;
using VSS.TRex.Common;
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
    /// Simple array to hold client leaf subgrid type map
    /// </summary>
    private static Type[] typeMap;

    /// <summary>
    /// Stores of cached client grids to reduce the object instantation and garbage collection overhead
    /// </summary>
//        private ConcurrentBag<IClientLeafSubGrid>[] ClientLeaves = Enumerable.Range(0, Enum.GetNames(typeof(GridDataType)).Length).Select(x => new ConcurrentBag<IClientLeafSubGrid>()).ToArray();
    private SimpleConcurrentBag<IClientLeafSubGrid>[] ClientLeaves = Enumerable.Range(0, Enum.GetNames(typeof(GridDataType)).Length).Select(x => new SimpleConcurrentBag<IClientLeafSubGrid>()).ToArray();

    /// <summary>
    /// Static class constructor to initialise types
    /// </summary>
    static ClientLeafSubGridFactory()
    {
      typeMap = new Type[]
      {
        null, // All = $00000000;
        null, // CCV = $00000001;
        null, // Height = $00000002;
        null, // Latency = $00000003;
        null, // PassCount = $00000004;
        null, // Frequency = $00000005;
        null, // Amplitude = $00000006;
        null, // Moisture = $00000007;
        null, // Temperature = $00000008;
        null, // RMV = $00000009;
        null, // GPSMode = $0000000A;
        null, // CCVPercent = $0000000B;
        null, // SimpleVolumeOverlay = $0000000C;
        null, // HeightAndTime = $0000000D;
        null, // CompositeHeights = $0000000E;
        null, // MDP = $0000000F;
        null, // MDPPercent = $00000010;
        null, // CellProfile = $00000011;
        null, // CellPasses = $00000012;
        null, // MachineSpeed = $00000013;
        null, // CCVPercentChange = $00000014;
        null, // MachineSpeedTarget = $00000015;
        null, // CCVPercentChangeIgnoredTopNullValue = $0000016
        null, // CCA = $0000017
        null, // CCAPerccent = $0000018
        null, // TemperatureDetail = 0x00000019
        null, // Unused 0x0000001A
        null, // Unused 0x0000001B
        null, // Unused 0x0000001C
        null, // Unused 0x0000001D
        null, // Unused 0x0000001E
        null, // Unused 0x0000001F
        null // CutFill = 0x00000020
      };
    }

    /// <summary>
    /// Register a type implementing IClientLeafSubGrid against a grid data type for the factory to 
    /// create on demand
    /// </summary>
    /// <param name="gridDataType"></param>
    /// <param name="type"></param>
    public void RegisterClientLeafSubGridType(GridDataType gridDataType, Type type)
    {
      // Check that the type being passed in meets the requirement for 
      // implementing the IClienLeafSubGrid interface
      if (!(typeof(IClientLeafSubGrid).IsAssignableFrom(type)))
      {
        throw new ArgumentException("ClientLeafSubGridFactory requires a type that implements IClientLeafSubGrid", "type");
      }

      if ((int) gridDataType > typeMap.Length)
      {
        throw new ArgumentException("Unknown grid data type in RegisterClientLeafSubgridType", "gridDataType");
      }

      typeMap[(int) gridDataType] = type;
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
          result = (IClientLeafSubGrid) Activator.CreateInstance(
            typeMap[(int) gridDataType], // IClientLeafSubGrid type
            null, // Subgrid tree owner
            null, // Subgrid parent
            SubGridTree.SubGridTreeLevels, // Level, default to standard tree levels
            SubGridTree.DefaultCellSize, // Cell Size
            SubGridTree.DefaultIndexOriginOffset // IndexOfiginOffset, default to tree default value
          );
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
      {
        return;
      }

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
        throw new ArgumentException("Invalid count of subgrids to return", "count");

      for (int i = 0; i < count; i++)
      {
        ReturnClientSubGrid(ref clientGrids[i]);
      }
    }

    /// <summary>
    /// Return an array of client grids (of the same type) previously obtained from the factory so it may reuse them
    /// </summary>
    /// <param name="clientGrids"></param>
    /// <param name="count"></param>
    public void ReturnClientSubGrids(IClientLeafSubGrid[][] clientGrids, int count)
    {
      if (count < 0 || count > clientGrids.Length)
        throw new ArgumentException("Invalid count of subgrids to return", "count");

      for (int i = 0; i < count; i++)
      {
        for (int j = 0; j < clientGrids[i]?.Length; j++)
        {
          ReturnClientSubGrid(ref clientGrids[i][j]);
        }
      }
    }
  }
}
