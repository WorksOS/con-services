using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.SubGridTrees.Server
{
    /// <summary>
    /// This is a static, immutable, version of the cell and cell passes that make up a sub grid segment
    /// </summary>
    [ExcludeFromCodeCoverage] // This class is no longer actively used,
    public class SubGridCellSegmentPassesDataWrapper_Static : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        /// <summary>
        /// CallPasses is a single collection of cell passes stored within this sub grid cell segment.
        /// </summary>
        private CellPass[] CellPasses;

        /// <summary>
        /// PassData is the collection of cells as present in this segment. These cells do not store their cell passes directly,
        /// instead they store references into the CellPasses array that stores all cell passes in the segment.
        /// </summary>
        public Cell_Static[,] PassData;

        /// <summary>
        /// Default no-arg constructor that does not instantiate any state
        /// </summary>
        public SubGridCellSegmentPassesDataWrapper_Static()
        {
        }

        /// <summary>
        /// Constructor that accepts a set of cell passes and creates the internal structures to hold them
        /// </summary>
        /// <param name="cellPasses"></param>
        /// <param name="cellPassCounts"></param>
        public void SetState(CellPass[,][] cellPasses, int[,] cellPassCounts)
        {
            // Ensure eny existing state is erased
            PassData = null;
            CellPasses = null;

            // Determine the total number of passes that need to be stored and create the array to hold them
            int totalPassCount = 0;
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                totalPassCount += cellPassCounts[i, j];
              }
            }

            CellPasses = new CellPass[totalPassCount];

            PassData = new Cell_Static[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            // Shift all the cell pass information into the private arrays, setting the cell pass offsets for 
            // each cell to reference the correct set of cell passes in the collated array.
            int runningPassCount = 0;

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                PassData[i, j].CellPassOffset = runningPassCount;

                var thisCellPasses = cellPasses[i, j];
                for (int cpIndex = 0, limit = cellPassCounts[i, j]; cpIndex < limit; cpIndex++)
                  CellPasses[runningPassCount++] = thisCellPasses[cpIndex];
              }
            }
        }

        /// <summary>
        /// Constructor that accepts a set of non static cells and creates the internal structures to hold them
        /// </summary>
        /// <param name="cells"></param>
        public SubGridCellSegmentPassesDataWrapper_Static(Cell_NonStatic[,] cells)
        {
            // Determine the total number of passes that need to be stored and create the array to hold them
            int totalPassCount = 0;

            foreach (Cell_NonStatic cell in cells)
            {
                totalPassCount += cell.PassCount;
            }

            CellPasses = new CellPass[totalPassCount];

            PassData = new Cell_Static[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            // Shift all the cell pass information into the private arrays, setting the cell pass offsets for 
            // each cell to reference the correct set of cell passes in the collated array.
            int runningPassCount = 0;

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                PassData[i, j].CellPassOffset = runningPassCount;

                var thisCellPasses = cells[i, j];
                for (int cpIndex = 0, limit = thisCellPasses.PassCount; cpIndex < limit; cpIndex++)
                  CellPasses[runningPassCount++] = thisCellPasses.Passes[cpIndex];
              }
            }
        }

        public int PassCount(int X, int Y)
        {
            return PassData[X, Y].PassCount;
        }

        public void AllocatePasses(int X, int Y, int passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void AddPass(int X, int Y, CellPass pass, int position = -1)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void ReplacePass(int X, int Y, int position, CellPass pass)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        public void RemovePass(int X, int Y, int position)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass ExtractCellPass(int X, int Y, int passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber];
        }

        /// <summary>
        /// Locates a cell pass occurring at or immediately after a given time within the passes for a specific cell within this segment.
        /// If there is not an exact match, the returned index is the location in the cell pass list where a cell pass 
        /// with the given time would be inserted into the list to maintain correct time ordering of the cell passes in that cell.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool LocateTime(int X, int Y, DateTime time, out int index)
        {
            return PassData[X, Y].LocateTime(CellPasses, time, out index);
        }

        public void Read(BinaryReader reader)
        {
            int TotalPasses = reader.ReadInt32();
            int MaxPassCount = reader.ReadInt32();

            int[,] PassCounts = new int[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            if (TotalPasses > 0)
            {
              for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  switch (PassCounts_Size)
                  {
                    case PassCountSize.ONE_BYTE:
                      PassCounts[i, j] = reader.ReadByte();
                      break;
                    case PassCountSize.TWO_BYTES:
                      PassCounts[i, j] = reader.ReadInt16();
                      break;
                    case PassCountSize.FOUR_BYTES:
                      PassCounts[i, j] = reader.ReadInt32();
                      break;
                    default:
                      throw new InvalidDataException($"Unknown PassCounts_Size {PassCounts_Size}");
                  }
                }
              }


              // Read all the cells from the stream
              for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  int PassCount_ = PassCounts[i, j];

                  if (PassCounts[i, j] > 0)
                  {
                    AllocatePasses(i, j, PassCounts[i, j]);
                    Read(i, j, reader);

                    segmentPassCount += PassCount_;
                  }
                }
              }
            }
        }

        private void Read(int X, int Y, BinaryReader reader)
        {
            int lastPassCount = PassData[X, Y].CellPassOffset + PassCount(X, Y);

            for (int cellPassIndex = PassData[X, Y].CellPassOffset; cellPassIndex < lastPassCount; cellPassIndex++)
            {
                CellPasses[cellPassIndex].Read(reader);
            }
        }

        private void Read(int X, int Y, int passNumber, BinaryReader reader)
        {
            CellPasses[PassData[X, Y].CellPassOffset + passNumber].Read(reader);
        }

        public void Write(int X, int Y, int passNumber, BinaryWriter writer)
        {
            CellPasses[PassData[X, Y].CellPassOffset + passNumber].Write(writer);
        }

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MinPassCount"></param>
        /// <param name="MaxPassCount"></param>
        public void CalculateTotalPasses(out int TotalPasses, out int MinPassCount, out int MaxPassCount)
        {
            SegmentTotalPassesCalculator.CalculateTotalPasses(this, out TotalPasses, out MinPassCount, out MaxPassCount);
        }

        /// <summary>
        /// Calculates the time range covering all the cell passes within this segment
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void CalculateTimeRange(out DateTime startTime, out DateTime endTime)
        {
            SegmentTimeRangeCalculator.CalculateTimeRange(this, out startTime, out endTime);
        }

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        public void CalculatePassesBeforeTime(DateTime searchTime, out int totalPasses, out int maxPassCount)
        {
            SegmentTimeRangeCalculator.CalculatePassesBeforeTime(this, searchTime, out totalPasses, out maxPassCount);
        }

        public void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime)
        {
            throw new ArgumentException("Static cell segment passes wrappers do not support cell pass adoption");
        }

        /// <summary>
        /// Returns a null machine ID set for nonstatic cell pass wrappers. MachineIDSets are an 
        /// optimization for read requests on compressed static cell pass representations
        /// </summary>
        /// <returns></returns>
        public BitArray GetMachineIDSet() => null;

        /// <summary>
        /// Sets the internal machine ID for the cell pass identified by x & y spatial location and passNumber.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passNumber"></param>
        /// <param name="internalMachineID"></param>
        public void SetInternalMachineID(int X, int Y, int passNumber, short internalMachineID)
        {
          throw new InvalidOperationException("Immutable cell pass segment.");
        }

        /// <summary>
        /// Sets the internal machine ID for all cell passes within the segment to the provided ID.
        /// </summary>
        /// <param name="internalMachineIndex"></param>
        /// <param name="numModifiedPasses"></param>
        public void SetAllInternalMachineIDs(short internalMachineIndex, out long numModifiedPasses)
        {
          throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public void GetSegmentElevationRange(out double MinElev, out double MaxElev)
        {
          MinElev = Consts.NullDouble;
          MaxElev = Consts.NullDouble;
        }

        public void Write(BinaryWriter writer)
        {
            CalculateTotalPasses(out int TotalPasses, out _, out int MaxPassCount);

            writer.Write(TotalPasses);
            writer.Write(MaxPassCount);

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            if (TotalPasses > 0)
            {
              // write all the pass counts to the stream
              for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  switch (PassCounts_Size)
                  {
                    case PassCountSize.ONE_BYTE:
                      writer.Write((byte) PassCount(i, j));
                      break;
                    case PassCountSize.TWO_BYTES:
                      writer.Write((short) PassCount(i, j));
                      break;
                    case PassCountSize.FOUR_BYTES:
                      writer.Write(PassCount(i, j));
                      break;
                    default:
                      throw new InvalidDataException($"Unknown PassCounts_Size: {PassCounts_Size}");
                  }
                }
              }

              // write all the cells to the stream
              for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  Write(i, j, writer);
                }
              }
            }
        }

        public void Write(int X, int Y, BinaryWriter writer)
        {
            int lastPassCount = PassData[X, Y].CellPassOffset + PassCount(X, Y);
            for (int cellPassIndex = PassData[X, Y].CellPassOffset; cellPassIndex < lastPassCount; cellPassIndex++)
            {
                CellPasses[cellPassIndex].Write(writer);
            }
        }

        public float PassHeight(int X, int Y, int passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber].Height;
        }

        public DateTime PassTime(int X, int Y, int passNumber)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passNumber].Time;
        }

        public void Integrate(int X, int Y, CellPass[] sourcePasses, int sourcePassCount, int StartIndex, int EndIndex, out int AddedCount, out int ModifiedCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass[] ExtractCellPasses(int X, int Y, out int passCount)
        {
            throw new InvalidOperationException("Immutable cell pass segment.");
        }

        public CellPass Pass(int X, int Y, int passIndex)
        {
            return CellPasses[PassData[X, Y].CellPassOffset + passIndex];
        }

        public CellPass[,][] GetState(out int[,] cellPassCounts)
        {
            throw new NotImplementedException("Does not support GetState()");
        }

        /// <summary>
        /// Note that this information is immutable
        /// </summary>
        /// <returns></returns>
        public bool IsImmutable() => true;

        public bool HasPassData() => PassData != null;

        public void ReplacePasses(int X, int Y, CellPass[] cellPasses, int cellPassCount)
        {
          throw new NotImplementedException("Does not support ReplacePasses()");
        }

        public void AllocatePassesExact(int X, int Y, int passCount)
        {
          throw new NotImplementedException("Does not support AllocatePassesExact()");
        }
    }
}
