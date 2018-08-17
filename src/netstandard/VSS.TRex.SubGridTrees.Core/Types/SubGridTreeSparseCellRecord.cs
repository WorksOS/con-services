using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Types
{
    /// <summary>
    /// Contains the location of, and reference to, a child cell within this subgrid.
    /// These items form a sparse list of child subgrids that is more space efficient than 
    /// maintaining a full 32x32 array of references where the vast majority are null 
    /// (no data in child subgrid)
    /// </summary>
    public struct SubgridTreeSparseCellRecord
    {
        /// <summary>
        /// X ordinate of the in-subgrid address of the cell
        /// </summary>
        public byte CellX { get; set; }

        /// <summary>
        /// Y ordinate of the in-subgrid address of the cell
        /// </summary>
        public byte CellY { get; set; }

        /// <summary>
        /// Reference to the cell at the X, Y location
        /// </summary>
        public ISubGrid Cell { get; set; }

        public SubgridTreeSparseCellRecord(byte cellX, byte cellY, ISubGrid cell)
        {
            CellX = cellX;
            CellY = cellY;
            Cell = cell;
        }
    }
}
