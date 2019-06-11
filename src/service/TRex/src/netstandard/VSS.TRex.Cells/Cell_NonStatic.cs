using System;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Cells
{
    /// <summary>
    /// Cell_NonStatic represents cell instances stored in the compaction information grid.
    /// The compaction information grid itself is modeled after the standard TGriddedGrid,
    /// but is reimplemented rather than derived.
    /// An interesting point to note is that a cell knows very little about it's context
    /// in the grid. It doesn't know who owns it, where it is or who its neighbors are.
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
        /// Number of cell passes recorded for this cell. It may be less than the actual size of Passes
        /// </summary>
        /// <returns></returns>
        public int PassCount;

        /// <summary>
        /// Determines if the cell is empty of all cell passes
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => PassCount == 0;

        /// <summary>
        /// Determines the height (Elevation from NEE) of the 'top most', or latest recorded in time, cell pass. If there are no passes a null height is returned.
        /// </summary>
        /// <returns></returns>
        public float TopMostHeight => IsEmpty ? CellPassConsts.NullHeight : Passes[PassCount - 1].Height;

        /// <summary>
        /// Allocate or resize an array of passes to a new size
        /// </summary>
        /// <param name="capacity"></param>
        public void AllocatePasses(int capacity)
        {
          const int CELL_PASS_ARRAY_INCREMENT_SIZE = 5;

          if (PassCount > capacity)
          {
            // Reset pass count to capacity, but don't reduce the allocated array size
            PassCount = capacity;
          }

          if (Passes == null)
          {
            Passes = capacity >= CELL_PASS_ARRAY_INCREMENT_SIZE ? new CellPass[capacity] : new CellPass[CELL_PASS_ARRAY_INCREMENT_SIZE];
          }
          else
          {
            int currentSize = Passes.Length;

            if (currentSize >= capacity)
            {
              // Current allocated capacity is sufficient.
              return;
            }

            if (capacity - currentSize >= CELL_PASS_ARRAY_INCREMENT_SIZE)
              Array.Resize(ref Passes, capacity);
            else
              Array.Resize(ref Passes, capacity + CELL_PASS_ARRAY_INCREMENT_SIZE);
          }
        }

        /// <summary>
        /// Allocate or resize an array of passes to a new size which will exactly equal the size asked for
        /// </summary>
        /// <param name="capacity"></param>
        public void AllocatePassesExact(int capacity)
        {
          if (PassCount == capacity)
          {
            // Current allocated capacity is correct
            return;
          }

          if (PassCount > capacity)
            PassCount = capacity;

          if (capacity == 0)
          {
            Passes = null;
          }
          else
          {
            if (Passes == null)
              Passes = new CellPass[capacity];
            else
              Array.Resize(ref Passes, capacity);
          }
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
                throw new TRexException("Pass with same time being added to cell");

            AllocatePasses(PassCount + 1);
            if (position < PassCount)
                Array.Copy(Passes, position, Passes, position + 1, PassCount - position);

            // Add the new pass to the passes list.
            Passes[position] = pass;
            PassCount++;

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
            if (PassCount > passIndex)
            {
              Array.Copy(Passes, passIndex + 1, Passes, passIndex, PassCount - passIndex - 1);
              PassCount--;

              // Don't reallocate the array to save allocation and additional copy.
              //AllocatePasses(PassCount - 1);
            }
        }

        /// <summary>
        /// Takes a cell pass stack and integrates its contents into this cell pass stack ensuring all duplicates are resolved
        /// and that cell pass ordering on time is preserved
        /// </summary>
        /// <param name="sourcePasses"></param>
        /// <param name="sourcePassCount"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="addedCount"></param>
        /// <param name="modifiedCount"></param>
        public void Integrate(CellPass[] sourcePasses,
                              int sourcePassCount,
                              int startIndex,
                              int endIndex,
                              out int addedCount,
                              out int modifiedCount)
        {
            addedCount = 0;
            modifiedCount = 0;

            if (sourcePassCount == 0)
                return;

            int ThisIndex = 0;
            int SourceIndex = startIndex;
            int IntegratedIndex = 0;

            int OriginalPassCount = PassCount;
            int IntegratedPassCount = OriginalPassCount + (endIndex - startIndex + 1);

            // Set the length to be the combined. While this may be more than needed if
            // there are passes in source that have identical times to the passes in
            // this cell pass stack, it does give an upper bound, and the minority of cases
            // where the actual number of passes are less than the total that are initially set here
            // will be cleaned up when the sub grid next exits the cache, or is integrated with
            // another aggregated sub grid from TAG file processing
            CellPass[] IntegratedPasses = new CellPass[IntegratedPassCount];

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
                    }

                IntegratedIndex++;
            } while (IntegratedIndex <= IntegratedPassCount - 1);

            // Don't resize the cell pass list downwards as this costs an allocation and array copy
            // This workflow is specific to TAG file ingest and the segments being manipulated are transient
            // and will be removed shortly in general operations.
            // if (IntegratedPasses.Length > IntegratedPassCount)
            //    Array.Resize(ref IntegratedPasses, (int)IntegratedPassCount);

            // Assign the integrated list of passes to this cell, replacing the previous list of passes.
            Passes = IntegratedPasses;
            PassCount = IntegratedPassCount;
            addedCount = IntegratedPassCount - OriginalPassCount;
        }

        public Cell_NonStatic(int cellPassCapacity)
        {
            Passes = null;
            PassCount = 0;
            AllocatePasses(cellPassCapacity);
        }
    }
}
