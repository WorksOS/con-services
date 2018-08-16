using System.Collections.Generic;
using VSS.TRex.Storage;

namespace VSS.TRex.GridFabric.Grids
{
  /// <summary>
  /// Represents information regarding the grids present in the TRex system
  /// </summary>
  public static class TRexGrids
  {
    /// <summary>
    /// The name of the grid containing mutable data
    /// </summary>
    public static string MutableGridName() => "TRex-Mutable";

    /// <summary>
    /// The name of the grid containing immutable CQRS projected data from the Mutable data grid
    /// </summary>
    public static string ImmutableGridName() => "TRex-Immutable";

    public static string GridName(StorageMutability Mutability) => Mutability == StorageMutability.Mutable ? MutableGridName() : ImmutableGridName();

    /// <summary>
    /// List of valid grid names
    /// </summary>
    /// <returns>A list of valid grid names</returns>
    public static IList<string> GridNames() => new List<string> { MutableGridName(), ImmutableGridName()};
  }
}
