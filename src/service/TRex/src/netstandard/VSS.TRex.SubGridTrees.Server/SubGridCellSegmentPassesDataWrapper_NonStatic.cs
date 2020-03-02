using System;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Compression;
using VSS.TRex.DI;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.SubGridTrees.Server
{
    public class SubGridCellSegmentPassesDataWrapper_NonStatic : SubGridCellSegmentPassesDataWrapperBase, ISubGridCellSegmentPassesDataWrapper
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridCellSegmentPassesDataWrapper_NonStatic>();

        /// <summary>
        /// A hook that may be used to gain notification of the add, replace and remove cell pass mutations in the cell pass stack
        /// </summary>
        private static readonly ICell_NonStatic_MutationHook MutationHook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

        private Cell_NonStatic[,] _passData;

        public SubGridCellSegmentPassesDataWrapper_NonStatic()
        {
          _passData = GenericTwoDArrayCacheHelper<Cell_NonStatic>.Caches().Rent(); //.RentEx(CellNonStaticRentValidator);
        }

        /// <summary>
        /// Checks if all cells in the 2D array being rented are correctly initialised for renting
        /// </summary>
        /// <param name="passData"></param>
        private void CellNonStaticRentValidator(Cell_NonStatic[,] passData)
        {
          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator
          ((x, y) =>
          {
            if (passData[x, y].Passes.IsRented)
            {
              throw new TRexException("Cell_NonStatic already rented in T[,].Rent()");
            }

            if (passData[x, y].Passes.Count != 0)
            {
              throw new TRexException("Cell_NonStatic pass count not zero in T[,].Rent()");
            }
          });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PassCount(int X, int Y) => _passData[X, Y].PassCount;

        /// <summary>
        /// Reduces the number of passes in the cell to newCount by preserving the first
        /// 'newCount' cell passes in the cell and retiring the remainder.
        /// If newCount is larger than the actual count an ArgumentException is thrown
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="newCount"></param>
        public void TrimPassCount(int X, int Y, int newCount)
        {
          if (newCount < 0 || newCount > _passData[X, Y].PassCount)
          {
            throw new ArgumentException($"newCount parameter ({newCount}) is less than zero or greater than than the number of passes in the cell ({_passData[X, Y].PassCount})");
          }

          _passData[X, Y].Passes.Count = newCount;

#if CELLDEBUG
          _passData[X, Y].CheckPassesAreInCorrectTimeOrder("TrimPassCount");
#endif
        }

        /// <summary>
        /// Ensures there are sufficient passes in the local cell pass array for this cell. Note: THe actual
        /// number of cell passes validly present in the cell may be less that the length of the cell pass array.
        /// Integrators must use the PassCount property to determine exactly how many passes are present.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="passCount"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AllocatePasses(int X, int Y, int passCount) => _passData[X, Y].AllocatePasses(passCount);
      
        public void AddPass(int X, int Y, CellPass pass)
        {
            if (pass.Time == Consts.MIN_DATETIME_AS_UTC || pass.Time.Kind != DateTimeKind.Utc)
              throw new ArgumentException("Cell passes added to cell pass stacks must have a non-null, UTC, cell pass time", nameof(pass.Time)); 

            MutationHook?.AddPass(X, Y, _passData[X, Y], pass);

            _passData[X, Y].AddPass(pass);

            segmentPassCount++;
        }

        public void ReplacePass(int X, int Y, int position, CellPass pass)
        {
            MutationHook?.ReplacePass(X, Y, _passData[X, Y], position, pass);

            _passData[X, Y].Passes.SetElement(pass, position);

#if CELLDEBUG
            _passData[X, Y].CheckPassesAreInCorrectTimeOrder("ReplacePass");
#endif
        }

        /// <summary>
        /// Removes a cell pass at a specific position within the cell passes for a cell in this segment. Only valid for mutable representations exposing this interface.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="position"></param>
        public void RemovePass(int X, int Y, int position)
        {
           MutationHook?.RemovePass(X, Y, position);
           //throw new NotImplementedException("Removal of cell passes is not yet supported");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CellPass ExtractCellPass(int X, int Y, int passNumber) => _passData[X, Y].Passes.GetElement(passNumber);

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool LocateTime(int X, int Y, DateTime time, out int index) => _passData[X, Y].LocateTime(time, out index);

        public void Read(BinaryReader reader)
        {
            segmentPassCount = reader.ReadInt32();

            var passCountBuffer = GenericArrayPoolCacheHelper<long>.Caches().Rent(SubGridTreeConsts.CellsPerSubGrid);
            try
            {
              var fieldDescriptor = new EncodedBitFieldDescriptor();
              fieldDescriptor.Read(reader);

              using (var bfa = new BitFieldArray())
              {
                bfa.Read(reader);
                int bitLocation = 0;
                bfa.ReadBitFieldVector(ref bitLocation, fieldDescriptor, SubGridTreeConsts.CellsPerSubGrid, passCountBuffer);
              }

              var counter = 0;

              for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  var passCount = (int)passCountBuffer[counter++];

                  if (passCount > 0)
                  {
                    AllocatePasses(i, j, passCount);

                    _passData[i, j].Passes.Count = passCount;
                    var passes = _passData[i, j].Passes;
                    for (int cpi = passes.Offset, limit = passes.OffsetPlusCount; cpi < limit; cpi++)
                    {
                      passes.Elements[cpi].Read(reader);
                    }
                  }
                }
              }
            }
            finally
            {
              GenericArrayPoolCacheHelper<long>.Caches().Return(ref passCountBuffer);
            }
        }

        /// <summary>
        /// Calculate the total number of passes from all the cells present in this sub grid segment
        /// </summary>
        /// <param name="totalPasses"></param>
        /// <param name="minPassCount"></param>
        /// <param name="maxPassCount"></param>
        public void CalculateTotalPasses(out int totalPasses, out int minPassCount, out int maxPassCount)
        {
          totalPasses = 0;
          maxPassCount = 0;
          minPassCount = int.MaxValue;

          if (!HasPassData())
            return;

          for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var thePassCount = _passData[i, j].PassCount;

              if (thePassCount > maxPassCount)
                maxPassCount = thePassCount;

              if (thePassCount < minPassCount)
                minPassCount = thePassCount;

              totalPasses += thePassCount;
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

          for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var passes = _passData[i, j].Passes;
              var elements = passes.Elements;

              for (int passIndex = passes.Offset, limit = passes.OffsetPlusCount; passIndex < limit; passIndex++)
              {
                var theTime = elements[passIndex].Time; 

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

          for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              var cell = _passData[i, j];
              var thePassCount = cell.PassCount;

              if (thePassCount == 0)
                continue;

              var countInCell = 0;

              // Implementation taking advantage of internal knowledge of TRexSpan
              var elements = cell.Passes.Elements;
              for (int passIndex = cell.Passes.Offset, limit = cell.Passes.OffsetPlusCount; passIndex < limit; passIndex++)
              {
                if (elements[passIndex].Time < searchTime)
                  countInCell++;
              }

              // Impleentation restricting itself to GetElement() API
              // for (var passIndex = 0; passIndex < thePassCount; passIndex++)
              // {
              //   var theTime = PassTime(i, j, passIndex);
              //   if (theTime < searchTime)
              //     countInCell++;
              // }

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
        var cellPasses = _passData[X, Y].Passes;
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

        for (var x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
        {
          for (var y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
          {
            var cellPasses = _passData[x, y].Passes;
            var elements = cellPasses.Elements;

            for (int i = cellPasses.Offset, limit = cellPasses.OffsetPlusCount; i < limit; i++)
              elements[i].InternalSiteModelMachineIndex = internalMachineIndex;

            numModifiedPasses += cellPasses.Count;
          }
        }
      }
     
      public void GetSegmentElevationRange(out double minElev, out double maxElev)
      {
        throw new TRexException("Elevation range determination for segments limited to STATIC_CELL_PASSES");
      }

      public void Write(BinaryWriter writer)
        {
            var totalPasses = 0;
            var passCountBuffer = GenericArrayPoolCacheHelper<long>.Caches().Rent(SubGridTreeConsts.CellsPerSubGrid);
            try
            {
              // Write all the cell to the stream
              // Assemble the pass counts, compress them and write them out as a single element
              var counter = 0;
              for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  var cellPassCount = _passData[i, j].PassCount;
                  passCountBuffer[counter++] = cellPassCount;
                  totalPasses += cellPassCount;
                }
              }

              writer.Write(totalPasses);

              var fieldDescriptor = new EncodedBitFieldDescriptor();
              AttributeValueRangeCalculator.CalculateAttributeValueRange(passCountBuffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid, 0xfff_ffff_ffff_ffff, 0, true, ref fieldDescriptor);
              fieldDescriptor.Write(writer);

              using (var bfa = new BitFieldArray())
              {
                var bitLocation = 0;
                
                bfa.Initialise(fieldDescriptor.RequiredBits, SubGridTreeConsts.CellsPerSubGrid);
                bfa.WriteBitFieldVector(ref bitLocation, fieldDescriptor, SubGridTreeConsts.CellsPerSubGrid, passCountBuffer);
                bfa.Write(writer);
              }

              if (_passData == null)
              {
                 Log.LogWarning($"****** Writing a nonstatic subgrid segment with no cell passes, counter = {counter}, totalPasses = {totalPasses}");
                 return;
              }

              for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
              {
                for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
                {
                  var cellPasses = _passData[i, j].Passes;
                  var passCount = cellPasses.Count;

                  // write all the cell passes to the stream, avoiding those cells that do not have any passes
                  if (passCount > 0)
                  {
#if CELLDEBUG
                    _passData[i, j].CheckPassesAreInCorrectTimeOrder("Writing cell passes");
#endif
                    for (int cpi = cellPasses.Offset, limit = cellPasses.OffsetPlusCount; cpi < limit; cpi++)
                    {
                      cellPasses.Elements[cpi].Write(writer);
                    }
                  }
                }
              }
            }
            finally
            {
              GenericArrayPoolCacheHelper<long>.Caches().Return(ref passCountBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float PassHeight(int X, int Y, int passNumber) => _passData[X, Y].Passes.GetElement(passNumber).Height;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime PassTime(int X, int Y, int passNumber) => _passData[X, Y].Passes.GetElement(passNumber).Time;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Integrate(int X, int Y, Cell_NonStatic sourcePasses, int StartIndex, int EndIndex, out int AddedCount, out int ModifiedCount)
        {
            _passData[X, Y].Integrate(sourcePasses, StartIndex, EndIndex, out AddedCount, out ModifiedCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Cell_NonStatic ExtractCellPasses(int X, int Y) => _passData[X, Y];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CellPass Pass(int X, int Y, int passIndex) => _passData[X, Y].Passes.GetElement(passIndex);

        public void SetState(Cell_NonStatic[,] cellPasses)
        {
          ReleaseCellPassesRental();

          segmentPassCount = 0;
          _passData = GenericTwoDArrayCacheHelper<Cell_NonStatic>.Caches().Rent(); //.RentEx(CellNonStaticRentValidator);

          for (var x = 0; x < SubGridTreeConsts.SubGridTreeDimension; x++)
          {
            for (var y = 0; y < SubGridTreeConsts.SubGridTreeDimension; y++)
            {
              var passes = cellPasses[x, y].Passes;

              _passData[x, y].Passes = GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Clone(passes);
#if CELLDEBUG
              _passData[x, y].CheckPassesAreInCorrectTimeOrder("SetState");
#endif
              segmentPassCount += passes.Count;
            }
          }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasPassData() => _passData != null;

        public bool IsImmutable() => false;

        public void ReplacePasses(int X, int Y, CellPass[] cellPasses, int cellPassCount)
        {
          GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Return(ref _passData[X, Y].Passes);

          var newPasses = GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Rent(cellPassCount);

          newPasses.Copy(cellPasses, cellPassCount);
          newPasses.Count = cellPassCount;

          _passData[X, Y].Passes = newPasses;

#if CELLDEBUG
          _passData[X, Y].CheckPassesAreInCorrectTimeOrder("ReplacePasses");
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Cell_NonStatic[,] GetState() => _passData;

    private void ReleaseCellPassesRental()
    {
      if (_passData == null)
      {
        return;
      }

      // Return all the rented TRexSpans in the current segment
      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          GenericSlabAllocatedArrayPoolHelper<CellPass>.Caches().Return(ref _passData[i, j].Passes);
        }
      }

      // ### DEBUG ###
      // Belt and braces, make sure the Cell_NonStatic passes look good.
      // CellNonStaticRentValidator(PassData);
      // ### DEBUG ###

      GenericTwoDArrayCacheHelper<Cell_NonStatic>.Caches().Return(ref _passData);
    }

    #region IDisposable Support
    private bool _disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        // Treat disposal and finalization as the same, dependent on the primary disposedValue flag
        ReleaseCellPassesRental();

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      Dispose(true);
    }
    #endregion
  }  
}
