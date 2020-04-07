using System;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// Provides a base class for iterating through cells in sub grids in a cell pass by cell pass manner.
    /// Separate iterators for static and non-static cell passes are derived from this class by overriding
    /// the abstract methods in the base class
    /// </summary>
    public abstract class SubGridSegmentCellPassIterator_Base : ISubGridSegmentCellPassIterator
    {
        // private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(SubGridSegmentCellPassIterator_Base));

        /// <summary>
        /// The sub grid relative cellX ordinate of the cell within which cell passes are being iterated
        /// </summary>
        public byte CellX { get; private set; }

        /// <summary>
        /// The sub grid relative cellY ordinate of the cell within which cell passes are being iterated
        /// </summary>
        public byte CellY { get; private set; }

        /// <summary>
        /// The iterator responsible for moving through the set of segments in the sub grid as the cell pass
        /// iterator moves through the set of cell passes
        /// </summary>
        public ISubGridSegmentIterator SegmentIterator { get; set; }

        protected int cellInSegmentIndex = -1;
        protected int finishCellInSegmentIndex = -1;

        /// <summary>
        /// Incrementer appropriate to the forwards or backwards direction of the iteration
        /// </summary>
        protected int cellPassIterationDirectionIncrement = 1;

        /// <summary>
        /// The minimum cell pass time the call passes iterated through will be returned
        /// </summary>
        public DateTime IteratorStartTime { get; private set; } = Consts.MIN_DATETIME_AS_UTC;

        /// <summary>
        /// The maximum cell pass time the call passes iterated through will be returned
        /// </summary>
        public DateTime IteratorEndTime { get; private set; } = Consts.MAX_DATETIME_AS_UTC;

        /// <summary>
        /// The date/time of the cell pass that was returned last from the iterator
        /// </summary>
        protected DateTime lastReturnedCellPassTime = Consts.MIN_DATETIME_AS_UTC;

        /// <summary>
        /// The maximum number of cell passes the iterator is permitted to return for a single cell
        /// </summary>
        public int MaxNumberOfPassesToReturn { get; set; } = int.MaxValue;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        protected SubGridSegmentCellPassIterator_Base()
        {
        }

        /// <summary>
        /// Construct a cell pass iterator using a given segment iterator and an optional maximum number of passes to return
        /// in the course of the iteration
        /// </summary>
        /// <param name="iterator"></param>
        /// <param name="maxNumberOfPassesToReturn"></param>
        protected SubGridSegmentCellPassIterator_Base(ISubGridSegmentIterator iterator,
            int maxNumberOfPassesToReturn = int.MaxValue) : this()
        {
            SegmentIterator = iterator;
            MaxNumberOfPassesToReturn = maxNumberOfPassesToReturn;
        }

        /// <summary>
        /// Moves to the next segment in the list of segments the cell pass iterator is iterating over
        /// </summary>
        protected void MoveToNextSegment()
        {
            SegmentIterator.MoveToNextSubGridSegment();

            if (SegmentIterator.CurrentSubGridSegment != null)
                InitialiseForNewSegment();
            else
                cellInSegmentIndex = finishCellInSegmentIndex;
        }

        /// <summary>
        /// Overridable method to initialise each new segment. Static and non-static cell pass representations
        /// provide overrides for this behaviour
        /// </summary>
        protected abstract void InitialiseForNewSegment(IterationDirection direction);

        /// <summary>
        /// Initializes the iterator state for the next segment to be iterated through. Static and non-static cell
        /// pass implementations override this appropriately.
        /// </summary>
        protected void InitialiseForNewSegment()
        {
            if (SegmentIterator.CurrentSubGridSegment == null)
              throw new TRexSubGridProcessingException("No current sub grid segment in iterator to initialise cell enumeration over.");

            InitialiseForNewSegment(SegmentIterator.IterationDirection);
        }

        // SetCellCoordinatesInSubGrid set the coordinates of the cell in the sub grid that
        // cell passes are being iterated over. The coordinates should be in the 0..DimensionSize-1 range
        public void SetCellCoordinatesInSubGrid(byte cellX, byte cellY)
        {
            if (cellX > SubGridTreeConsts.SubGridTreeDimensionMinus1 || cellY > SubGridTreeConsts.SubGridTreeDimensionMinus1)
              throw new TRexSubGridProcessingException($"Cell coordinates out of range in {nameof(SetCellCoordinatesInSubGrid)}");

            CellX = cellX;
            CellY = cellY;
        }

        /// <summary>
        /// Sets the range of elevations for which cell passes will be returned in this iteration
        /// </summary>
        /// <param name="minElevation"></param>
        /// <param name="maxElevation"></param>
        public void SetIteratorElevationRange(double minElevation, double maxElevation) =>
            SegmentIterator.SetIteratorElevationRange(minElevation, maxElevation);

        /// <summary>
        /// Initialise the cell pass iterator using the segment iterator given to it
        /// </summary>
        public void Initialise()
        {
            if (SegmentIterator == null)
                throw new TRexSubGridProcessingException("No segment iterator assigned");

            lastReturnedCellPassTime = SegmentIterator.IterationDirection == IterationDirection.Forwards
              ? Consts.MIN_DATETIME_AS_UTC
              : Consts.MAX_DATETIME_AS_UTC;

            SegmentIterator.InitialiseIterator();
            SegmentIterator.MoveToFirstSubGridSegment();

            if (SegmentIterator.CurrentSubGridSegment != null)
                InitialiseForNewSegment();
        }

        /// <summary>
        /// Abstract method to extract the current cell pass in the iteration. Static and non-static cell pass
        /// implementations override this method
        /// </summary>
        /// <returns></returns>
        protected abstract CellPass ExtractCellPass();

        /// <summary>
        /// Gets the next cell pass in the iteration
        /// </summary>
        /// <param name="cellPass"></param>
        /// <returns></returns>
        public bool GetNextCellPass(ref CellPass cellPass)
        {
          if (SegmentIterator.CurrentSubGridSegment == null)
            return false;

          DateTime cellPassTime;
          do
          {
            cellInSegmentIndex += cellPassIterationDirectionIncrement;

            while (cellInSegmentIndex == finishCellInSegmentIndex && SegmentIterator.CurrentSubGridSegment != null)
            {
              MoveToNextSegment();

              cellInSegmentIndex += cellPassIterationDirectionIncrement;
            }

            if (SegmentIterator.CurrentSubGridSegment == null)
              return false; // No more segments to process

            cellPass = ExtractCellPass();
            cellPassTime = cellPass.Time;

            if (cellPassTime == Consts.MIN_DATETIME_AS_UTC)
              throw new TRexSubGridProcessingException($"Cell pass with null time returned from {nameof(GetNextCellPass)}");

            if (SegmentIterator.IterationDirection == IterationDirection.Forwards)
            {
              if (cellPassTime > IteratorEndTime)
                return false;
            }
            else
            {
              if (cellPassTime < IteratorStartTime)
                return false;
            }
          } while (cellPassTime < IteratorStartTime || cellPassTime > IteratorEndTime);

          lastReturnedCellPassTime = cellPassTime;
          return true;
        }

        /// <summary>
        /// MayHaveMoreFilterableCellPasses indicates that there may be additional cell passes
        /// to be filtered from the cell. Note the _may_ - this is not explicitly
        /// indicating that there definitely are more cell passes. If this function returns
        /// false, then there are definitely no more cells that could be filtered from
        /// this cell. At the current time, this is governed by any appropriate time
        /// restriction set from a filter        
        /// </summary>
        /// <returns></returns>
        public bool MayHaveMoreFilterableCellPasses()
        {
            return SegmentIterator.IterationDirection == IterationDirection.Forwards
                ? lastReturnedCellPassTime < IteratorEndTime
                : lastReturnedCellPassTime > IteratorStartTime;
        }

        /// <summary>
        /// Sets the time range bounds for the cell pass filter to return relevant cell passes for 
        /// </summary>
        /// <param name="hasTimeFilter"></param>
        /// <param name="iteratorStartTime"></param>
        /// <param name="iteratorEndTime"></param>
        public void SetTimeRange(bool hasTimeFilter, DateTime iteratorStartTime, DateTime iteratorEndTime)
        {
            IteratorStartTime = hasTimeFilter ? iteratorStartTime: Consts.MIN_DATETIME_AS_UTC;
            IteratorEndTime = hasTimeFilter ? iteratorEndTime : Consts.MAX_DATETIME_AS_UTC;

            // if we have a attached segment iterator then also set its date range
            SegmentIterator?.SetTimeRange(iteratorStartTime, iteratorEndTime);
        }
    }
}
