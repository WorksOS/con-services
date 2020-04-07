using System;
using System.Collections;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
    /// <summary>
    /// TSubGridSegmentIteratorStateIndex records iteration progress across a sub grid
    /// </summary>
    public class IteratorStateIndex : IIteratorStateIndex
    {
        public DateTime StartSegmentTime { get; set; } = Consts.MIN_DATETIME_AS_UTC;
        public DateTime EndSegmentTime { get; set; } = Consts.MAX_DATETIME_AS_UTC;

        public IterationDirection IterationDirection { get; set; } = IterationDirection.Forwards;

        public IServerLeafSubGrid SubGrid { get; set; }

        private ISubGridDirectory _directory;

        public ISubGridDirectory Directory
        {
            get => _directory;
            
            set
            {
                _directory = value;

                if (_directory != null)
                {
                    _initialNumberOfSegments = _directory.SegmentDirectory.Count;
                }
            }
        }

        private int _initialNumberOfSegments;

        public double MinIterationElevation { get; set; } = Consts.NullDouble;
        public double MaxIterationElevation { get; set; } = Consts.NullDouble;

        private bool _restrictSegmentIterationBasedOnElevationRange;

        // The current index of the segment at this point in the iteration
        public int Idx { get; set; }

        public bool HasMachineRestriction = false;

        public BitArray MachineIDSet { get; set; }

        // The sub grid whose segments are being iterated across

        public void Initialise()
        {
            _initialNumberOfSegments = _directory.SegmentDirectory.Count;

            Idx = IterationDirection == IterationDirection.Forwards ? -1 : _initialNumberOfSegments;
        }

        public bool NextSegment()
        {
            bool result;

            if (_initialNumberOfSegments != _directory.SegmentDirectory.Count)
                throw new TRexSubGridProcessingException("Number of segments in sub grid has changed since the iterator was initialized");

            do
            {
                Idx = IterationDirection == IterationDirection.Forwards ? ++Idx : --Idx;
                var segmentIndexInRange = Range.InRange(Idx, 0, _directory.SegmentDirectory.Count - 1);

                if (!segmentIndexInRange)
                  return false;

                var segmentInfo = _directory.SegmentDirectory[Idx];

                result = Range.InRange(segmentInfo.StartTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(segmentInfo.EndTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(StartSegmentTime, segmentInfo.StartTime, segmentInfo.EndTime) ||
                         Range.InRange(EndSegmentTime, segmentInfo.StartTime, segmentInfo.EndTime);

                // If there is an elevation range restriction is place then check to see if the
                // segment contains any cell passes in the elevation range
                if (result && _restrictSegmentIterationBasedOnElevationRange)
                {
                  if (segmentInfo.MinElevation != Consts.NullDouble && segmentInfo.MaxElevation != Consts.NullDouble)
                  {
                    result = Range.InRange(segmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                             Range.InRange(segmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                             Range.InRange(MinIterationElevation, segmentInfo.MinElevation,
                               segmentInfo.MaxElevation) ||
                             Range.InRange(MaxIterationElevation, segmentInfo.MinElevation, segmentInfo.MaxElevation);
                  }
                  else if (segmentInfo.Segment?.PassesData != null)
                  {
                    // The elevation range information we use here is accessed via
                    // the entropic compression information used to compress the attributes held
                    // in the segment. If the segment has not been loaded yet then this information
                    // is not available. In this case don't perform the test, but allow the segment
                    // to be loaded and the passes in it processed according to the current filter.
                    // If the segment has been loaded then access this information and determine
                    // if there is any need to extract cell passes from this segment. If not, just move
                    // to the next segment

                    segmentInfo.Segment.PassesData.GetSegmentElevationRange(out var segmentMinElev, out var segmentMaxElev);
                    if (segmentMinElev != Consts.NullDouble && segmentMaxElev != Consts.NullDouble)
                    {
                      // Save the computed elevation range values for this segment
                      segmentInfo.MinElevation = segmentMinElev;
                      segmentInfo.MaxElevation = segmentMaxElev;

                      result =
                        Range.InRange(segmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                        Range.InRange(segmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                        Range.InRange(MinIterationElevation, segmentInfo.MinElevation, segmentInfo.MaxElevation) ||
                        Range.InRange(MaxIterationElevation, segmentInfo.MinElevation, segmentInfo.MaxElevation);
                    }
                    else
                    {
                      result = false;
                    }
                  }

                  if (result && HasMachineRestriction && segmentInfo.Segment?.PassesData != null)
                  {
                    // Check to see if this segment has any machines that match the
                    // machine restriction. If not, advance to the next segment
                    var hasMachinesOfInterest = false;
                    var segmentMachineIdSet = segmentInfo.Segment.PassesData.GetMachineIDSet();

                    if (segmentMachineIdSet != null)
                    {
                      for (var i = 0; i < MachineIDSet.Count; i++)
                      {
                        hasMachinesOfInterest = MachineIDSet[i] && segmentMachineIdSet[i];
                        if (hasMachinesOfInterest)
                          break;
                      }

                      result = hasMachinesOfInterest;
                    }
                  }
                }
            }
            while (!result);

            return true;
        }

        public bool AtLastSegment()
        {
            if (IterationDirection == IterationDirection.Forwards)
            {
                return (Idx >= _directory.SegmentDirectory.Count - 1) ||
                         (_directory.SegmentDirectory[Idx + 1].StartTime > EndSegmentTime);
            }

            return (Idx <= 0) || (Directory.SegmentDirectory[Idx - 1].EndTime <= StartSegmentTime);
        }

        public void SetTimeRange(DateTime startSegmentTime, DateTime endSegmentTime)
        {
            StartSegmentTime = startSegmentTime;
            EndSegmentTime = endSegmentTime;
        }

        public void SetIteratorElevationRange(double minIterationElevation, double maxIterationElevation)
        {
            MinIterationElevation = minIterationElevation;
            MaxIterationElevation = maxIterationElevation;

            _restrictSegmentIterationBasedOnElevationRange = (minIterationElevation != Consts.NullDouble) && (MaxIterationElevation != Consts.NullDouble);
        }

        public void SetMachineRestriction(BitArray machineIdSet)
        {
            MachineIDSet = machineIdSet;
        }

        public void SegmentListExtended()
        {
            if (IterationDirection != IterationDirection.Forwards)
                throw new TRexSubGridProcessingException("Extension of segment list only valid if iterator is traveling forwards through the list");

            // Reset the number of segments now expected in the segment.
            _initialNumberOfSegments = _directory.SegmentDirectory.Count;
        }
    }
}
