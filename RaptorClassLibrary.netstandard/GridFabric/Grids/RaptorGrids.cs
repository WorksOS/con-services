using VSS.VisionLink.Raptor.Storage;

namespace VSS.VisionLink.Raptor.GridFabric.Grids
{
    /// <summary>
    /// Represents information regarding the grids present in the Raptor system
    /// </summary>
    public static class RaptorGrids
    {
        /// <summary>
        /// The name of the core grid within Raptor
        /// </summary>
//        public static string RaptorGridName() => "Raptor";

        /// <summary>
        /// The name of the grid containing mutable data
        /// </summary>
        public static string RaptorMutableGridName() => "Raptor-Mutable";

        /// <summary>
        /// The name of the grid containing immutable CQRS projected data from the Mutable data grid
        /// </summary>
        public static string RaptorImmutableGridName() => "Raptor-Immutable";

        public static string RaptorGridName(StorageMutability Mutability) => Mutability == StorageMutability.Mutable ? RaptorMutableGridName() : RaptorImmutableGridName();
    }
}
