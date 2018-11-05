using System;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client.Interfaces
{
    /// <summary>
    /// Factory interface used to implement concrete client leaf subgrid factory for use by processing
    /// and other business logic that needs to create client leaf subgrids of varying kinds.
    /// This factory may will eventually use dependency injection to derive the varieties of leaf subgrids
    /// that can be produced
    /// </summary>
    public interface IClientLeafSubgridFactory
    {
        /// <summary>
        /// Register a type implementing IClientLeafSubGrid against a grid data type for the factory to 
        /// create on demand
        /// </summary>
        /// <param name="gridDataType"></param>
        /// <param name="constructor"></param>
        void RegisterClientLeafSubGridType(GridDataType gridDataType, Func<IClientLeafSubGrid> constructor);

        /// <summary>
        /// Construct a concrete instance of a subgrid implementing the IClientLeafSubGrid interface based
        /// on the role it should play according to the grid data type requested. All aspects of leaf ownership
        /// by a subgrid tree, parentage, level, cell size, index origin offset are delegated responsibilities
        /// of the caller or a derived factory class
        /// </summary>
        /// <param name="gridDataType"></param>
        /// <returns>An appropriate instance derived from ClientLeafSubgrid</returns>
        IClientLeafSubGrid GetSubGrid(GridDataType gridDataType);

        void ReturnClientSubGrid(ref IClientLeafSubGrid clientGrid);
        void ReturnClientSubGrids(IClientLeafSubGrid[] clientGrid, int count);
        void ReturnClientSubGrids(IClientLeafSubGrid[][] clientGrid, int count);
    }
}
