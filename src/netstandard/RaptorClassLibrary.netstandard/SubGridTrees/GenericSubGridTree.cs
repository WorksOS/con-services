using System;
using System.Diagnostics;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.SubGridTrees
{
    /// <summary>
    /// GenericSubGridTree in T implements a subgrid tree where all the cells in the leaf subgrids are generic type T.
    /// The tree automates tree node and leaf subgrid management behind a uniform tree wide Cells[,] facade.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericSubGridTree<T> : SubGridTree
    {
        /// <summary>
        /// Default indexer property to access the cells as a default property of the generic subgrid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public T this[uint x, uint y] { get { return GetCell(x, y); } set { SetCell(x, y, value); } }

        /// <summary>
        /// Generic cell value setter for the sub grid tree. 
        /// Setting a value for a cell automatically creates all necessary node & leaf subgrids to
        /// store the value.
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <returns></returns>
        private T GetCell(uint cellX, uint cellY)
        {
            ISubGrid subGrid = LocateSubGridContaining(cellX, cellY, NumLevels);

            if (subGrid == null)
            {
                return NullCellValue;
            }

            subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);
            return ((GenericLeafSubGrid<T>)subGrid).Items[subGridX, subGridY];
        }

        /// <summary>
        /// Generic cell value getter for the sub grid tree.
        /// Getting a value for a cell automatically traverses the tree to locate the appropriate leaf subgrid
        /// to return the value from.
        /// If there is no leaf subgrid, or the value in the leaf subgrid is nul, this function returns the value
        /// represented by NullCellValue().
        /// </summary>
        /// <param name="cellX"></param>
        /// <param name="cellY"></param>
        /// <param name="value"></param>
        private void SetCell(uint cellX, uint cellY, T value)
        {
            ISubGrid subGrid = ConstructPathToCell(cellX, cellY, SubGridPathConstructionType.CreateLeaf);

            if (subGrid == null)
            {
                Debug.Assert(false, "Unable to create cell subgrid");
            }
            else
            {
                subGrid.GetSubGridCellIndex(cellX, cellY, out byte subGridX, out byte subGridY);
                ((GenericLeafSubGrid<T>)subGrid).Items[subGridX, subGridY] = value;
            }
        }

        /// <summary>
        /// NullGetCellValue is the null value leaf cell values stored in this generic sub grid tree.
        /// Descendants should override this method. Calling it directly will result in the standard .Net
        /// default value for type T being returned.
        /// </summary>
        /// <returns></returns>
        public virtual T NullCellValue => default(T);

        /// <summary>
        /// Generic sub grid tree constructor. Accepts standard cell size, number of levels and the 
        /// factory for creation of new node and leaf subgrids
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        /// <param name="subGridfactory"></param>
        public GenericSubGridTree(byte numLevels,
                                  double cellSize,
                                  ISubGridFactory subGridfactory) : base(numLevels, cellSize, subGridfactory)
        {
        }

        /// <summary>
        /// Generic sub grid tree constructor. Accepts the standard cell size, number of levels; however,
        /// the sub grid factory is created from the standard NodeSubGrid class, and the base generic leaf subgrid
        /// derived from T. Note: This is only suitable if the default(T) value is appropriate for the cell null value.
        /// </summary>
        /// <param name="numLevels"></param>
        /// <param name="cellSize"></param>
        public GenericSubGridTree(byte numLevels,
                                  double cellSize) : base(numLevels, cellSize, new SubGridFactory<NodeSubGrid, GenericLeafSubGrid<T>>())
        {
        }

        /// <summary>
        /// Iterates over all leaf cell values in the entire subgrid tree. All leaf subgrids are
        /// iterated over and all values (both null and non-null) in the leaf subgrid are presented
        /// to the functor.
        /// </summary>
        /// <param name="functor"></param>
        public void ForEach(Func<T, bool> functor)
        {
            ScanAllSubGrids(subgrid =>
            {
                ((GenericLeafSubGrid<T>)subgrid).ForEach(functor);
                return true;
            });
        }
    }
}
