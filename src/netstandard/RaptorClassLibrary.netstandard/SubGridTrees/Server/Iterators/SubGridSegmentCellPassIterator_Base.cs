using System;
using System.Diagnostics;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Utilities;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// Provides a base class for iterating through cells in subgrids in a cell pass by cell pass manner.
    /// Separate iterators for static and non-static cell passes are derived from this class by overriding
    /// the abstract functors in the base class
    /// </summary>
    public abstract class SubGridSegmentCellPassIterator_Base : ISubGridSegmentCellPassIterator
    {
        /// <summary>
        /// The subgrid relative cellX ordiante of the cell within which cell passes are being iterated
        /// </summary>
        protected byte cellX = byte.MaxValue;

        /// <summary>
        /// The subgrid relative cellX ordinate of the cell within which cell passes are being iterated
        /// </summary>
        public byte CellX { get { return cellX; } }

        /// <summary>
        /// The subgrid relative cellY ordinate of the cell within which cell passes are being iterated
        /// </summary>
        protected byte cellY = byte.MaxValue;

        /// <summary>
        /// The subgrid relative cellY ordinate of the cell within which cell passes are being iterated
        /// </summary>
        public byte CellY { get { return cellY; } }

        /// <summary>
        /// The iterator responsible for moving through the set of segments in the subgrid as the cell pass
        /// iterator moves through the set of cell passes
        /// </summary>
        public ISubGridSegmentIterator SegmentIterator { get; set; }

        protected int cellInSegmentIndex = -1;
        protected int finishCellInSegmentIndex = -1;

        /// <summary>
        /// Incrementor appropriate to the forwards or backwards direction of the iteration
        /// </summary>
        protected int cellPassIterationDirectionIncrement = 1;

        /// <summary>
        /// The minimum cell pass time the call passes iterated through will be returned
        /// </summary>
        protected DateTime iteratorStartTime = DateTime.MinValue;

        /// <summary>
        /// The minimum cell pass time the call passes iterated through will be returned
        /// </summary>
        public DateTime IteratorStartTime { get { return iteratorStartTime; } }

        /// <summary>
        /// The maximum cell pass time the call passes iterated through will be returned
        /// </summary>
        protected DateTime iteratorEndTime = DateTime.MaxValue;

        /// <summary>
        /// The maximum cell pass time the call passes iterated through will be returned
        /// </summary>
        public DateTime IteratorEndTime { get { return iteratorEndTime; } }

        /// <summary>
        /// The date/time of the cell pass that was returned last from the iterator
        /// </summary>
        protected DateTime lastReturnedCellPassTime = DateTime.MinValue;

        /// <summary>
        /// The maximum number of cell passes the iterator is permitted to return for a single cell
        /// </summary>
        public int MaxNumberOfPassesToReturn { get; set; } = int.MaxValue;

        //        protected SiteModel SiteModelReference { get; set; } = null;

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public SubGridSegmentCellPassIterator_Base()
        {
        }

        /// <summary>
        /// Construct a cell pass iterator using a given segment iterator and an optional maximum number of passes to return
        /// in the course of the iteration
        /// </summary>
        /// <param name="iterator"></param>
        /// <param name="maxNumberOfPassesToReturn"></param>
        public SubGridSegmentCellPassIterator_Base(ISubGridSegmentIterator iterator, int maxNumberOfPassesToReturn = int.MaxValue) : this()
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
            {
                InitialiseForNewSegment();
            }
            else
            {
                cellInSegmentIndex = finishCellInSegmentIndex;
            }
        }

        /// <summary>
        /// Overidable method to initialise each new segment. Static and non-static cell pass representatins
        /// provide overides for this behaviour
        /// </summary>
        protected abstract void InitialiseForNewSegment(IterationDirection direction);

        /// <summary>
        /// Initialises the iterator state for the next segment to be iterated through. Static and non-static cell
        /// paas implementations override this appropriately.
        /// </summary>
        protected void InitialiseForNewSegment()
        {
            if (SegmentIterator.CurrentSubGridSegment == null)
            {
                // TODO add when logging is available
                //SIGLogMessage.PublishNoODS(Self, 'No current subgrid segment in iterator to initialise cell enumeration over.', slmcAssert);
                return;
            }

            InitialiseForNewSegment(SegmentIterator.IterationDirection);
        }

        // SetCellCoordinatesInSubgrid set the coordinates of the cell in the subgrid that
        // cell passes are being iterated over. The coordinates should be in the 0..DimensionSize-1 range
        public void SetCellCoordinatesInSubgrid(byte _cellX, byte _cellY)
        {
            Debug.Assert(Range.InRange(_cellX, (byte)0, (byte)(SubGridTree.SubGridTreeDimensionMinus1)) &&
                         Range.InRange(_cellY, (byte)0, (byte)(SubGridTree.SubGridTreeDimensionMinus1)),
                         "Cell coordinates out of range in SetCellCoordinatesInSubgrid");

            cellX = _cellX;
            cellY = _cellY;
        }

        /// <summary>
        /// Sets the range of elevations for which cell passes will be returned in this iteration
        /// </summary>
        /// <param name="minElevation"></param>
        /// <param name="maxElevation"></param>
        public void SetIteratorElevationRange(double minElevation, double maxElevation) => SegmentIterator.SetIteratorElevationRange(minElevation, maxElevation);

        /// <summary>
        /// Initialise the cell pass iterator using the segment iterator given to it
        /// </summary>
        public void Initialise()
        {
            if (SegmentIterator == null)
            {
                // TODO add whenlogging available
                // SIGLogMessage.PublishNoODS(Self, 'No segment iterator assigned', slmcAssert);
                return;
            }

            lastReturnedCellPassTime = SegmentIterator.IterationDirection == IterationDirection.Forwards ? DateTime.MinValue : DateTime.MaxValue;

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
        /// <param name="CellPass"></param>
        /// <returns></returns>
        public bool GetNextCellPass(ref CellPass CellPass)
        {
            DateTime CellPassTime;

            if (SegmentIterator.CurrentSubGridSegment == null)
            {
                return false; // No more cells to process
            }

            Debug.Assert(Range.InRange(cellX, (byte)0, (byte)(SubGridTree.SubGridTreeDimensionMinus1)) &&
                         Range.InRange(cellY, (byte)0, (byte)(SubGridTree.SubGridTreeDimensionMinus1)),
                         "Cell coordinates out of range in GetNextCellPass");

            do
            {
                cellInSegmentIndex += cellPassIterationDirectionIncrement;

                while (cellInSegmentIndex == finishCellInSegmentIndex && SegmentIterator.CurrentSubGridSegment != null)
                {
                    MoveToNextSegment();

                    cellInSegmentIndex += cellPassIterationDirectionIncrement;
                }

                if (SegmentIterator.CurrentSubGridSegment == null)
                {
                    return false; // No more segments to process
                }

                CellPass = ExtractCellPass();
                CellPassTime = CellPass.Time;

                Debug.Assert(CellPassTime > DateTime.MinValue, "Cell pass with null time returned from SubGridSegmentCellPassIterator.GetNextCellPass");

                if (SegmentIterator.IterationDirection == IterationDirection.Forwards)
                {
                    if (CellPassTime > iteratorEndTime)
                    {
                        return false;
                    }
                }
                else
                {
                    if (CellPassTime < iteratorStartTime)
                    {
                        return false;
                    }
                }
            }
            while (CellPassTime < iteratorStartTime || CellPassTime > iteratorEndTime);

            lastReturnedCellPassTime = CellPassTime;

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
            return SegmentIterator.IterationDirection == IterationDirection.Forwards ? lastReturnedCellPassTime < iteratorEndTime : lastReturnedCellPassTime > iteratorStartTime;
        }

        /// <summary>
        /// Sets the time range bounds for the cell pass filter to return relevant cell passes for 
        /// </summary>
        /// <param name="hasTimeFilter"></param>
        /// <param name="iteratorStartTime"></param>
        /// <param name="iteratorEndTime"></param>
        public void SetTimeRange(bool hasTimeFilter, DateTime iteratorStartTime, DateTime iteratorEndTime)
        {
            if (hasTimeFilter)
            {
                this.iteratorStartTime = iteratorStartTime;
                this.iteratorEndTime = iteratorEndTime;
            }
            else
            {
                this.iteratorStartTime = DateTime.MinValue;
                this.iteratorEndTime = DateTime.MaxValue;
            }

            // if we have a attached segment iterator then also set its date range
            SegmentIterator?.SetTimeRange(iteratorStartTime, iteratorEndTime);
        }

        // Machine restriction not implemented
        //      public void SetMachineRestriction(const AMachineIDSets : TMachineIDSets)
        //            {
        //              FSegmentIterator.SetMachineRestriction(AMachineIDSets);
        //            }
    }
}
