using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.IO;
using VSS.TRex.IO.Helpers;

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
        private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(Cell_NonStatic));

        public const int PASS_COUNT_INCREMENT_STEP_SIZE = 4;

        /// <summary>
        /// Passes represents all the passes a compactor has made over this cell in the
        /// compaction information grid. The passes are arranged in time order: The first
        /// entry representing the oldest value, the last cell representing the most
        /// current reading.
        /// </summary>

        public TRexSpan<CellPass> Passes;

        /// <summary>
        /// Number of cell passes recorded for this cell. It may be less than the actual size of Passes
        /// </summary>
        /// <returns></returns>
        public int PassCount => Passes.Count;

        /// <summary>
        /// Determines if the cell is empty of all cell passes
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty => Passes.Count == 0;

        /// <summary>
        /// Determines the height (Elevation from NEE) of the 'top most', or latest recorded in time, cell pass. If there are no passes a null height is returned.
        /// </summary>
        /// <returns></returns>
        public float TopMostHeight => IsEmpty ? CellPassConsts.NullHeight : Passes.Last().Height;

        /// <summary>
        /// Allocate or resize an array of passes to a new size, with additional space provided for expansion
        /// </summary>
        /// <param name="capacity"></param>
        public void AllocatePasses(int capacity)
        {
          if (!Passes.IsRented)
          {
            Passes = GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Rent(capacity);

#if CELLDEBUG
            if (!Passes.IsRented)
            {
              throw new Exception("Is not rented!");
            }
#endif
            return;
          }

          if (Passes.Count >= capacity)
          {
#if CELLDEBUG
            if (!Passes.IsRented)
            {
              throw new Exception("Is not rented!");
            }
#endif
            // Current allocated capacity is sufficient.
            Passes.Count = capacity;
            return;
          }

          if (capacity > Passes.Capacity)
          {
            // Get a new buffer and copy the content into it
            var newPasses = GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Rent(capacity);

            newPasses.Copy(Passes, Passes.Count);
            GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Return(ref Passes);

            Passes = newPasses;

#if CELLDEBUG
            if (!Passes.IsRented)
            {
              throw new Exception("Is not rented!");
            }
#endif
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
            int L = Passes.Offset;
            int H = Passes.OffsetPlusCount - 1;

            while (L <= H)
            {
                int I = (L + H) >> 1;
                int C = Passes.Elements[I].Time.CompareTo(time);

                if (C < 0)
                {
                    L = I + 1;
                }
                else
                {
                    if (C == 0)
                    {
                        index = I - Passes.Offset;
                        return true;
                    }

                    H = I - 1;
                }
            }

            index = L - Passes.Offset;

            return false;
        }

        /// <summary>
        /// AddPass takes a pass record containing pass information processed
        /// for a machine crossing this cell and adds it to the passes list
        /// </summary>
        /// <param name="pass"></param>
        public void AddPass(CellPass pass)
        {
#if CELLDEBUG
            CheckPassesAreInCorrectTimeOrder("AddPass(CellPass pass) - before");
            pass._additionStamp = Interlocked.Increment(ref CellPass._lastAdditionStamp);
#endif

            // Locate the position in the list of time ordered passes to insert the new pass
            if (LocateTime(pass.Time, out int position))
            {
              throw new TRexException("Pass with same time being added to cell");
            }

            if (!Passes.IsRented)
            {
              AllocatePasses(PASS_COUNT_INCREMENT_STEP_SIZE);
            }
            else if (Passes.Capacity == Passes.Count)
            {
              AllocatePasses(Passes.Capacity + PASS_COUNT_INCREMENT_STEP_SIZE);
            }

            if (position < PassCount)
            {
              Passes.Insert(pass, position);
#if CELLDEBUG
              CheckPassesAreInCorrectTimeOrder("AddPass(CellPass pass) - after insert");
#endif
            }
            else // Add the new pass to the passes list.
            {
              Passes.Add(pass);
#if CELLDEBUG
              CheckPassesAreInCorrectTimeOrder("AddPass(CellPass pass) - after add");
#endif
            }
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
        public void Integrate(Cell_NonStatic sourcePasses, 
                              int startIndex,
                              int endIndex,
                              out int addedCount,
                              out int modifiedCount)
        {
#if CELLDEBUG
            // Check 'this' cell pass times are in order
            CheckPassesAreInCorrectTimeOrder("Cell passes are not in time order before integration");
            sourcePasses.CheckPassesAreInCorrectTimeOrder("Source cell passes are not in time order before integration");
#endif

            addedCount = 0;
            modifiedCount = 0;

            if (sourcePasses.Passes.Count == 0)
            {
              return;
            }

            var thisIndex = 0;
            var sourceIndex = startIndex;
            var integratedIndex = 0;

            var originalPassCount = PassCount;
            var integratedPassCount = originalPassCount + (endIndex - startIndex + 1);

            // Set the length to be the combined. While this may be more than needed if
            // there are passes in source that have identical times to the passes in
            // this cell pass stack, it does give an upper bound, and the minority of cases
            // where the actual number of passes are less than the total that are initially set here
            // will be cleaned up when the sub grid next exits the cache, or is integrated with
            // another aggregated sub grid from TAG file processing
            
            var integratedPasses = GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Rent(integratedPassCount);
          
            // Combine the two (sorted) lists of cell passes together to arrive at a single
            // integrated list of passes.
            do
            {
                if (thisIndex >= PassCount)
                {
                  integratedPasses.Add(sourcePasses.Passes.GetElement(sourceIndex));
                  sourceIndex++;
                }
                else if (sourceIndex > endIndex)
                {
                  integratedPasses.Add(Passes.GetElement(thisIndex));
                  thisIndex++;
                }
                else
                {
                  var thisElement = Passes.GetElement(thisIndex);
                  var sourceElement = sourcePasses.Passes.GetElement(sourceIndex);

                  switch (thisElement.Time.CompareTo(sourceElement.Time))
                    {
                        case -1:
                            {
                                integratedPasses.Add(thisElement);
                                thisIndex++;
                                break;
                            }
                        case 0:
                            {
                                if (!thisElement.Equals(sourceElement))
                                    modifiedCount++;

                                integratedPasses.Add(sourceElement);
                                sourceIndex++;
                                thisIndex++;
                                integratedPassCount--;
                                break;
                            }
                        case 1:
                            {
                                integratedPasses.Add(sourceElement);
                                sourceIndex++;
                                break;
                            }
                    }
                }

                integratedIndex++;
            } while (integratedIndex <= integratedPassCount - 1);

            integratedPasses.Count = integratedPassCount;

            // Assign the integrated list of passes to this cell, replacing the previous list of passes.
            // Return the original cell pass span and replace it with the integrated one
            GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Return(ref Passes);

            // No need to mark Passes as being returned as it is immediately replace by IntegratedPasses below
            // Passes.MarkReturned();
            Passes = integratedPasses;

            addedCount = integratedPassCount - originalPassCount;

#if CELLDEBUG
            CheckPassesAreInCorrectTimeOrder("Cell passes are not in time order after integration");
#endif
        }

#if CELLDEBUG
        /// <summary>
        /// Determines if all the passes in the cell are in the correct (increasing) time order
        /// </summary>
        /// <returns></returns>
        public void CheckPassesAreInCorrectTimeOrder(string comment)
        {
          for (int i = Passes.Offset, limit = Passes.OffsetPlusCount - 1; i < limit; i++)
          {
            if (Passes.Elements[i].Time >= Passes.Elements[i + 1].Time)
            {
               Log.LogInformation($"CheckPassesAreInCorrectTimeOrder failure [{comment}]: {Passes.Count} passes: {Passes.Offset}->{Passes.OffsetPlusCount - 1}");
               for (int j = Passes.Offset, limit2 = Passes.OffsetPlusCount; j < limit2; j++)
               {
                 Log.LogInformation($"Pass index {j}: Stamp {Passes.Elements[j]._additionStamp} Time: {Passes.Elements[j].Time}");
               }

               throw new Exception($"{comment}: {Passes.Elements[i].Time.Ticks} should be < {Passes.Elements[i].Time.Ticks}");
            }
          }
        }
#endif
    }
}
