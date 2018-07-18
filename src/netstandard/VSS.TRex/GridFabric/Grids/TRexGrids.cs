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
    }
}
