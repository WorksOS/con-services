using System;
using System.Diagnostics;
using System.Linq;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.Cells
{
    /// <summary>
    /// Cell_NonStatic represents cell instances stored in the compaction information grid.
    /// The compaction information grid itself is modelled after the standard TGriddedGrid,
    /// but is reimplemented rather than derived.
    /// An interesting point to note is that a cell knows very little about it's context
    /// in the grid. It doesn't know who owns it, where it is or who its neighbours are.
    /// These are all handled in upper layers which must provide such information to the
    /// cell as needed when requesting the cell perform certain operations or calculate
    /// certain quantities (such as calculating the current topmost height of the cell).
    /// </summary>
    public struct Cell_NonStatic
    {
        /// <summary>
        /// Passes represents all the passes a compactor has made over this cell in the
        /// compaction information grid. The passes are arranged in time order: The first
        /// entry representing the oldest value, the last cell representing the most
        /// current reading.
        /// </summary>
        public CellPass[] Passes;

        /// <summary>
        /// Number of cell passes recorded for this cell
        /// </summary>
        /// <returns></returns>
        public uint PassCount => Passes == null ? 0 : (uint)Passes.Length;

        /// <summary>
        /// Determines if the cell is empty of all cell passes
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => PassCount == 0;

        /// <summary>
        /// Determines the height (Elevation from NEE) of the 'top most', or latest recorded in time, cell pass. If there are no passes a null height is returned.
        /// </summary>
        /// <returns></returns>
        public float TopMostHeight => IsEmpty ? CellPassConsts.NullHeight : Passes.Last().Height;

        /// <summary>
        /// Allocate or resize an array of passes to a new size
        /// </summary>
        /// <param name="passCount"></param>
        public void AllocatePasses(uint passCount)
        {
            Array.Resize(ref Passes, (int)passCount);
        }

        /// <summary>
        /// LocateTime attempts to locate an entry in the passes list that has
        /// the same time stamp as the Time parameter
        /// It uses a binary search to locate any matching pass. As there will
        /// only ever by a single pass that matches, finding an exact match
        /// aborts the binary search and returns the result. If there is no
        /// exact match the search returns the index in the list where a pass with
        /// the given time should go. 
        /// </summary>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(DateTime time, out int index)
        {
            int L = 0;
            int H = (int)PassCount - 1;

            while (L <= H)
            {
                int I = (L + H) >> 1;
                int C = Passes[I].Time.CompareTo(time);

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

        /// <summary>
        /// AddPass takes a pass record containing pass information processed
        /// for a machine crossing this cell and adds it to the passes list
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="position"></param>
        public void AddPass(CellPass pass, int position = -1)
        {
            // Locate the position in the list of time ordered passes to insert the new pass
            if (position == -1 && LocateTime(pass.Time, out position))
            {
                Debug.Assert(false, "Pass with same time being added to cell");
            }

            // Yes, this looks naive, however, this operation really only occurs during TAG
            // file processing where the cell pass results from that processing are added into
            // new, and temporary, subgrid trees.
            // Examination of the code in the previous version showed this the effective behaviour anyway.

            AllocatePasses(PassCount + 1);
            if (position < PassCount - 1)
            {
                Array.Copy(Passes, position, Passes, position + 1, PassCount - position - 1);
            }

            // Add the new pass to the passes list.
            Passes[position] = pass;

#if CELLDEBUG
            for (int i = 0; i < PassCount - 1; i++)
            {
                Debug.Assert(Passes[i].Time < Passes[i + 1].Time, "Passes not in time order during cell processing.");
            }
#endif
        }

        /// <summary>
        /// ReplacePass takes a pass record containing pass information processed
        /// for a machine crossing this cell and replaces the pass at the given
        /// position in the cell pass stack
        /// </summary>
        /// <param name="position"></param>
        /// <param name="pass"></param>
        public void ReplacePass(int position, CellPass pass)
        {
            Passes[position] = pass;
        }

        /// <summary>
        /// Removes the pass at the given index from the list of passes, and resizes the resulting array
        /// </summary>
        /// <param name="passIndex"></param>
        public void RemovePass(int passIndex)
        {
            Debug.Assert(PassCount > passIndex, "Attempt to remove non-existant pass");

            if (PassCount > passIndex)
            {
                Array.Copy(Passes, passIndex + 1, Passes, passIndex, PassCount - passIndex - 1);
            }

            AllocatePasses(PassCount - 1);
        }

        /// <summary>
        /// Takes a cell pass stack and integrates its contents into this cell pass stack ensuring all duplicates are resolved
        /// and that cell pass ordering on time is preserved
        /// </summary>
        /// <param name="sourcePasses"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="addedCount"></param>
        /// <param name="modifiedCount"></param>
        public void Integrate(CellPass[] sourcePasses,
                              uint startIndex,
                              uint endIndex,
                              out int addedCount,
                              out int modifiedCount)
        {
            addedCount = 0;
            modifiedCount = 0;

            if (sourcePasses.Length == 0)
            {
                return;
            }

            CellPass[] IntegratedPasses = null;
            int ThisIndex = 0;
            uint SourceIndex = startIndex;
            int IntegratedIndex = 0;

            int OriginalPassCount = (int)PassCount;
            int IntegratedPassCount = (int)(OriginalPassCount + (endIndex - startIndex + 1));

            // Set the length to be the combined. While this may be more than needed if
            // there are passes in source that have identical times to the passes in
            // this cell pass stack, it does give an upper bound, and the minority of cases
            // where the actual number of passes are less than the total that are initially set here
            // will be cleaned up when the subgrid next exits the cache, or is integrated with
            // another aggregated subgrid from TAG file processing
            Array.Resize(ref IntegratedPasses, IntegratedPassCount);

            // Combine the two (sorted) lists of cell passes together to arrive at a single
            // integrated list of passes.
            do
            {
                if (ThisIndex >= PassCount)
                {
                    IntegratedPasses[IntegratedIndex] = sourcePasses[SourceIndex];
                    SourceIndex++;
                }
                else if (SourceIndex > endIndex)
                {
                    IntegratedPasses[IntegratedIndex] = Passes[ThisIndex];
                    ThisIndex++;
                }
                else switch (Passes[ThisIndex].Time.CompareTo(sourcePasses[SourceIndex].Time))
                    {
                        case -1:
                            {
                                IntegratedPasses[IntegratedIndex] = Passes[ThisIndex];
                                ThisIndex++;
                                break;
                            }
                        case 0:
                            {
                                if (!Passes[ThisIndex].Equals(sourcePasses[SourceIndex]))
                                    modifiedCount++;

                                IntegratedPasses[IntegratedIndex] = sourcePasses[SourceIndex];
                                SourceIndex++;
                                ThisIndex++;
                                IntegratedPassCount--;
                                break;
                            }
                        case 1:
                            {
                                IntegratedPasses[IntegratedIndex] = sourcePasses[SourceIndex];
                                SourceIndex++;
                                break;
                            }
                        default:
                            {
                                Debug.Assert(false, "Invalid result from CompareTo");
                                break;
                            }
                    }

                IntegratedIndex++;
            } while (IntegratedIndex <= IntegratedPassCount - 1);

            // Assign the integrated list of passes to this cell, replacing the previous list of passes.
            if (IntegratedPasses.Length > IntegratedPassCount)
            {
                Array.Resize(ref IntegratedPasses, IntegratedPassCount);
            }
            else
            {
                Debug.Assert(IntegratedPasses.Length == IntegratedPassCount, "Integrated pass count lists not same length");
            }

            Passes = IntegratedPasses;
            addedCount = IntegratedPassCount - OriginalPassCount;
        }

        public Cell_NonStatic(uint cellPassCount)
        {
            Passes = null;
            AllocatePasses(cellPassCount);
        }
    }
}
