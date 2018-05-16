using System;

namespace VSS.TRex.Cells
{
    /// <summary>
    /// Cell_Static represents cell instances stored in the compaction information grid.
    /// The compaction information grid itself is modelled after the standard TGriddedGrid,
    /// but is reimplemented rather than derived.
    /// An interesting point to note is that a cell knows very little about it's context
    /// in the grid. It doesn't know who owns it, where it is or who its neighbours are.
    /// These are all handled in upper layers which must provide such information to the
    /// cell as needed when requesting the cell perform certain operations or calculate
    /// certain quantities (such as calculating the current topmost height of the cell).
    /// Note: Static cells do not store their own cell passes; these are delegated to the 
    /// owning subgrid to store. Similarly, as the cell is static, it is an immutable construct
    /// and may not be added to or otherwise modified once constructed.
    /// </summary>
    public struct Cell_Static
    {
        /// <summary>
        /// Static cells do not record cell passes within their own structure but delegate this to the subgrid that owns the
        /// cell. CellPassOffset records the location in that wider set of cell passes within the subgrid where the list of cell passes
        /// recorded for this cell starts. 
        /// </summary>
        public uint CellPassOffset;

        /// <summary>
        /// Number of cell passes recorded for this cell
        /// </summary>
        /// <returns></returns>
        public uint PassCount;

        /// <summary>
        /// Determines if the cell is empty of all cell passes
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => PassCount == 0;

        /// <summary>
        /// Determines the height (Elevation from NEE) of the 'top most', or latest recorded in time, cell pass. 
        /// If there are no passes a null height is returned. 
        /// A reference to the total list of cell passes the passes for this cell is stored within is passed into this method
        /// </summary>
        /// <returns></returns>
        public float TopMostHeight(CellPass[] Passes) => IsEmpty ? CellPass.NullHeight : Passes[CellPassOffset + PassCount].Height;

        /// <summary>
        /// LocateTime attempts to locate an entry in the passes list that has
        /// the same time stamp as the Time parameter
        /// It uses a binary search to locate any matching pass. As there will
        /// only ever by a single pass that matches, finding an exact match
        /// aborts the binary search and returns the result. If there is no
        /// exact match the search returns the index in the list where a pass with
        /// the given time should go. 
        /// </summary>
        /// <param name="Passes"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(CellPass[] Passes, DateTime time, out int index)
        {
            int L = (int)CellPassOffset;
            int H = L + (int)PassCount - 1;

            while (L <= H)
            {
                var I = (L + H) >> 1;

                var C = Passes[I].Time.CompareTo(time);

                if (C < 0)
                {
                    L = I + 1;
                }
                else
                {
                    if (C == 0)
                    {
                        index = I;
                        return true;
                    }

                    H = I - 1;
                }
            }

            index = L;
            return false;
        }

        public Cell_Static(uint cellPassOffset, uint cellPassCount)
        {
            CellPassOffset = cellPassOffset;
            PassCount = cellPassCount;
        }
    }
}
