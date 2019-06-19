using System;
using System.Collections;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.Cells.Helpers;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Common.Utilities;
using VSS.TRex.DI;

namespace VSS.TRex.SubGridTrees.Server
{
    public class SubGridCellSegmentPassesDataWrapper_NonStatic : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper, IDisposable
    {
        /// <summary>
        /// A hook that may be used to gain notification of the add, replace and remove cell pass mutations in the cell pass stack
        /// </summary>
        private static readonly ICell_NonStatic_MutationHook _mutationHook = DIContext.Obtain<ICell_NonStatic_MutationHook>();
     
        private Cell_NonStatic[,] PassData = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

        public SubGridCellSegmentPassesDataWrapper_NonStatic()
        {
        }

        public int PassCount(int X, int Y) => PassData[X, Y].PassCount;

        /// <summary>
        /// Ensures there are sufficient passes in the local cell pass array for this cell. Note: THe actual
        /// number of cell passes validly present in the cell may be less that the length of the cell pass array.
        /// Integrators must use the PassCount property to determine exactly how many passes are present.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        public void AllocatePasses(int X, int Y, int passCount) => PassData[X, Y].AllocatePasses(passCount);

        /// <summary>
        /// Ensures there are sufficient passes in the local cell pass array for this cell. The exact number of
        /// cell passes will be created as asked for.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        public void AllocatePassesExact(int X, int Y, int passCount) => PassData[X, Y].AllocatePassesExact(passCount);

        public void AddPass(int X, int Y, CellPass pass, int position = -1)
        {
            if (pass.Time == Consts.MIN_DATETIME_AS_UTC || pass.Time.Kind != DateTimeKind.Utc)
              throw new ArgumentException("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time", nameof(pass.Time)); 

            _mutationHook?.AddPass(X, Y, PassData[X, Y], pass, position);

            PassData[X, Y].AddPass(pass, position);

            segmentPassCount++;
        }

        public void ReplacePass(int X, int Y, int position, CellPass pass)
        {
            _mutationHook?.ReplacePass(X, Y, PassData[X, Y], position, pass);

            PassData[X, Y].Passes.SetElement(pass, position);
        }

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        public void RemovePass(int X, int Y, int position)
        {
           _mutationHook?.RemovePass(X, Y, position);
           //throw new NotImplementedException("Removal of cell passes is not yet supported");
        }

        public CellPass ExtractCellPass(int X, int Y, int passNumber) => PassData[X, Y].Passes.GetElement(passNumber);

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
        public bool LocateTime(int X, int Y, DateTime time, out int index) => PassData[X, Y].LocateTime(time, out index);

        public void Read(BinaryReader reader)
        {
            int TotalPasses = reader.ReadInt32();
            int MaxPassCount = reader.ReadInt32();

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                int passCount;

                switch (PassCounts_Size)
                {
                  case PassCountSize.ONE_BYTE:
                    passCount = reader.ReadByte();
                    break;
                  case PassCountSize.TWO_BYTES:
                    passCount = reader.ReadInt16();
                    break;
                  case PassCountSize.FOUR_BYTES:
                    passCount = reader.ReadInt32();
                    break;
                  default:
                    throw new InvalidDataException($"Unknown PassCounts_Size {PassCounts_Size}");
                }

                if (passCount > 0)
                {
                  AllocatePassesExact(i, j, passCount);

                  var readerPass = new CellPass();

                  for (int cellPassIndex = 0; cellPassIndex < passCount; cellPassIndex++)
                  {
                    readerPass.Read(reader);
                    PassData[i, j].Passes.Add(readerPass);
                  }

                  segmentPassCount += passCount;
                }
              }
            }
        }

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="TotalPasses"></param>
        /// <param name="MinPassCount"></param>
        /// <param name="MaxPassCount"></param>
        public void CalculateTotalPasses(out int TotalPasses, out int MinPassCount, out int MaxPassCount)
        {
          TotalPasses = 0;
          MaxPassCount = 0;
          MinPassCount = int.MaxValue;

          if (!HasPassData())
            return;

          for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              int ThePassCount = PassData[i, j].PassCount;

              if (ThePassCount > MaxPassCount)
                MaxPassCount = ThePassCount;

              if (ThePassCount < MinPassCount)
                MinPassCount = ThePassCount;

              TotalPasses += ThePassCount;
            }
          }
        }

        /// <summary>
        /// Calculates the time range covering all the cell passes within this segment
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public void CalculateTimeRange(out DateTime startTime, out DateTime endTime)
        {
          startTime = Consts.MAX_DATETIME_AS_UTC;
          endTime = Consts.MIN_DATETIME_AS_UTC;

          for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var passes = PassData[i, j].Passes;
              var elements = passes.Elements;

              for (int PassIndex = passes.Offset, limit = passes.Offset + passes.Count; PassIndex < limit; PassIndex++)
              {
                var theTime = elements[PassIndex].Time; 

                if (theTime > endTime)
                  endTime = theTime;

                if (theTime < startTime)
                  startTime = theTime;
              }
            }
          }
        }

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        public void CalculatePassesBeforeTime(DateTime searchTime, out int totalPasses, out int maxPassCount)
        {
          totalPasses = 0;
          maxPassCount = 0;

          for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var cell = PassData[i, j];
              int thePassCount = cell.PassCount;

              if (thePassCount == 0)
                continue;

              int countInCell = 0;

              for (int PassIndex = 0; PassIndex < thePassCount; PassIndex++)
              {
                var theTime = PassTime(i, j, PassIndex);

                if (theTime < searchTime)
                  countInCell++;
              }

              totalPasses += countInCell;

              if (countInCell > maxPassCount)
                maxPassCount = countInCell;
            }
          }
        }

        /// <summary>
        /// Causes this segment to adopt all cell passes from sourceSegment where those cell passes were 
        /// recorded at or later than a specific date
        /// </summary>
        /// <param name="sourceSegment"></param>
        /// <param name="atAndAfterTime"></param>
        public void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper sourceSegment, DateTime atAndAfterTime)
        {
            SegmentCellPassAdopter.AdoptCellPassesFrom(this, sourceSegment, atAndAfterTime);
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
        var cellPasses = PassData[X, Y].Passes;
        cellPasses.Elements[cellPasses.Offset + passNumber].InternalSiteModelMachineIndex = internalMachineID;
      }

      /// <summary>
      /// Sets the internal machine ID for all cell passes within the segment to the provided ID.
      /// </summary>
      /// <param name="internalMachineIndex"></param>
      /// <param name="numModifiedPasses"></param>
      public void SetAllInternalMachineIDs(short internalMachineIndex, out long numModifiedPasses)
      {
        numModifiedPasses = 0;

        for (int x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
        {
          for (int y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
          {
            int passCount = PassCount(x, y);

            var cellPasses = PassData[x, y].Passes;
            var elements = cellPasses.Elements;

            for (int i = cellPasses.Offset, limit = cellPasses.OffsetPlusCount; i < limit; i++)
              elements[i].InternalSiteModelMachineIndex = internalMachineIndex;

            numModifiedPasses += passCount;
          }
        }
      }
     
      public void GetSegmentElevationRange(out double MinElev, out double MaxElev)
      {
        throw new TRexException("Elevation range determination for segments limited to STATIC_CELL_PASSES");
      }

      public void Write(BinaryWriter writer)
        {
            CalculateTotalPasses(out int TotalPasses, out _, out int MaxPassCount);

            writer.Write(TotalPasses);
            writer.Write(MaxPassCount);

            int PassCounts_Size = PassCountSize.Calculate(MaxPassCount);

            // Read all the cells from the stream
            for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                var passCount = PassData[i, j].PassCount;

                switch (PassCounts_Size)
                {
                  case PassCountSize.ONE_BYTE:
                    writer.Write((byte)passCount);
                    break;
                  case PassCountSize.TWO_BYTES:
                    writer.Write((short)passCount);
                    break;
                  case PassCountSize.FOUR_BYTES:
                    writer.Write(passCount);
                    break;
                  default:
                    throw new InvalidDataException($"Unknown PassCounts_Size: {PassCounts_Size}");
                }

                // write all the cell passes to the stream, avoiding those cells that do not have any passes
                if (passCount > 0)
                {
                  var cellPasses = PassData[i, j].Passes;

                  for (int cpi = 0; cpi < passCount; cpi++)
                  {
                    cellPasses.GetElement(cpi).Write(writer);
                  }
                }
              }
            }
        }

        public float PassHeight(int X, int Y, int passNumber) => PassData[X, Y].Passes.GetElement(passNumber).Height;

        public DateTime PassTime(int X, int Y, int passNumber) => PassData[X, Y].Passes.GetElement(passNumber).Time;

        public void Integrate(int X, int Y, Cell_NonStatic sourcePasses, int StartIndex, int EndIndex, out int AddedCount, out int ModifiedCount)
        {
            PassData[X, Y].Integrate(sourcePasses, StartIndex, EndIndex, out AddedCount, out ModifiedCount);
        }

        public Cell_NonStatic ExtractCellPasses(int X, int Y) => PassData[X, Y];

        public CellPass Pass(int X, int Y, int passIndex) => PassData[X, Y].Passes.GetElement(passIndex);

        public void SetState(Cell_NonStatic[,] cellPasses)
        {
          segmentPassCount = 0;
          PassData = new Cell_NonStatic[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

          for (int x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
          {
            for (int y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
            {
              var passes = cellPasses[x, y].Passes;

              PassData[x, y].Passes = SlabAllocatedCellPassArrayPoolHelper.Caches.Clone(passes);

              segmentPassCount += passes.Count;
            }
          }
        }

        public bool HasPassData() => PassData != null;

        public bool IsImmutable() => false;

        public void ReplacePasses(int X, int Y, CellPass[] cellPasses, int cellPassCount)
        {
          var oldPasses = PassData[X, Y].Passes;
          if (oldPasses.NeedsToBeReturned())
          {
            SlabAllocatedCellPassArrayPoolHelper.Caches.Return(oldPasses);
          }

          var newPasses = SlabAllocatedCellPassArrayPoolHelper.Caches.Rent(cellPassCount);
          newPasses.Copy(cellPasses, cellPassCount);
          PassData[X, Y].Passes = newPasses;
        }

        public Cell_NonStatic[,] GetState() => PassData;

    #region IDisposable Support
    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        // Treat disposal and finalization as the same, dependent on the primary disposedValue flag

        if (PassData != null)
        {
          // Return all the rented TRexSpans in the current segment
          for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var cellPasses = PassData[i, j];

              if (cellPasses.Passes.NeedsToBeReturned())
              {
                SlabAllocatedCellPassArrayPoolHelper.Caches.Return(cellPasses.Passes);
                cellPasses.Passes.MarkReturned();
              }
            }
          }
        }

        disposedValue = true;
      }
    }

    ~SubGridCellSegmentPassesDataWrapper_NonStatic()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }  
}
