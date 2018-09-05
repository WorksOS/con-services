using System;
using System.Collections;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Utilities;

namespace VSS.TRex.SubGridTrees.Server.Iterators
{
  /// <summary>
    /// TSubGridSegmentIteratorStateIndex records iteration progress across a subgrid
    /// </summary>
    public class IteratorStateIndex : IIteratorStateIndex
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(nameof(IteratorStateIndex));

        public DateTime StartSegmentTime { get; set; } = DateTime.MinValue;
        public DateTime EndSegmentTime { get; set; } = DateTime.MaxValue;

        public IterationDirection IterationDirection { get; set; } = IterationDirection.Forwards;

        public IServerLeafSubGrid SubGrid { get; set; }

        private ISubGridDirectory _Directory;

        public ISubGridDirectory Directory
        {
            get
            {
                return _Directory;
            }
            set
            {
                _Directory = value;

                if (_Directory != null)
                {
                    InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
                }
            }
        }

        private int InitialNumberOfSegments;

        public double MinIterationElevation { get; set; } = Consts.NullDouble;
        public double MaxIterationElevation { get; set; } = Consts.NullDouble;

        private bool RestrictSegmentIterationBasedOnElevationRange;

        // The current index of the segment at this point in the iteration
        public int Idx { get; set; }

        public bool HasMachineRestriction = false;

        public BitArray MachineIDSet { get; set; } = null;

        //public SiteModel SiteModelReference { get; set; } = null;

        // The subgrid whose segments are being iterated across

        public void Initialise()
        {
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;

            Idx = IterationDirection == IterationDirection.Forwards ? -1 : InitialNumberOfSegments;
        }

        public bool NextSegment()
        {
            // double SegmentMinElev, SegmentMaxElev;

            bool Result;

            if (InitialNumberOfSegments != _Directory.SegmentDirectory.Count)
            {
                Log.LogCritical("Number of segments in subgrid has changed since the iterator was initialised");
                return false;
            }

            do
            {
                Idx = IterationDirection == IterationDirection.Forwards ? ++Idx : --Idx;

                bool SegmentIndexInRange = Range.InRange(Idx, 0, _Directory.SegmentDirectory.Count - 1);

                if (!SegmentIndexInRange)
                {
                    return false;
                }

                ISubGridCellPassesDataSegmentInfo SegmentInfo = _Directory.SegmentDirectory[Idx];

                Result = Range.InRange(SegmentInfo.StartTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(SegmentInfo.EndTime, StartSegmentTime, EndSegmentTime) ||
                         Range.InRange(StartSegmentTime, SegmentInfo.StartTime, SegmentInfo.EndTime) ||
                         Range.InRange(EndSegmentTime, SegmentInfo.StartTime, SegmentInfo.EndTime);

                // If there is an elevation range restriction is place then check to see if the
                // segment contains any cell passes in the elevation range
                if (Result && RestrictSegmentIterationBasedOnElevationRange)
                {
                    if (SegmentInfo.MinElevation != Consts.NullDouble && SegmentInfo.MaxElevation != Consts.NullDouble)
                    {
                        Result = Range.InRange(SegmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                                 Range.InRange(SegmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                                 Range.InRange(MinIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation) ||
                                 Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                    }
                    else
                      if (SegmentInfo.Segment?.PassesData != null)
                    {
                        // The elevation range information we use here is accessed via
                        // the entropic compression information used to compress the attributes held
                        // in the segment. If the segment has not been loaded yet then this information
                        // is not available. In this case don't perform the test, but allow the segment
                        // to be loaded and the passes in it processed according to the current filter.
                        // If the segment has been loaded then access this information and determine
                        // if there is any need to extract cell passes from this segment. If not, just move
                        // to the next segment

                        SegmentInfo.Segment.PassesData.GetSegmentElevationRange(out double SegmentMinElev, out double SegmentMaxElev);
                        if (SegmentMinElev != Consts.NullDouble && SegmentMaxElev != Consts.NullDouble)
                        {
                            // Save the computed elevation range values for this segment
                            SegmentInfo.MinElevation = SegmentMinElev;
                            SegmentInfo.MaxElevation = SegmentMaxElev;

                            Result = Range.InRange(SegmentInfo.MinElevation, MinIterationElevation, MaxIterationElevation) ||
                                     Range.InRange(SegmentInfo.MaxElevation, MinIterationElevation, MaxIterationElevation) ||
                                     Range.InRange(MinIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation) ||
                                     Range.InRange(MaxIterationElevation, SegmentInfo.MinElevation, SegmentInfo.MaxElevation);
                        }
                        else
                        {
                            Result = false;
                        }
                        //*/
                    }

                    if (Result && HasMachineRestriction && SegmentInfo.Segment?.PassesData != null)
                    {
                        // Check to see if this segment has any machines that match the
                        // machine restriction. If not, advance to the next segment
                        bool HasMachinesOfInterest = false;
                        BitArray segmentMachineIDSet = SegmentInfo.Segment.PassesData.GetMachineIDSet();

                        if (segmentMachineIDSet != null)
                        {
                            for (int i = 0; i < MachineIDSet.Count; i++)
                            {
                                HasMachinesOfInterest = MachineIDSet[i] && segmentMachineIDSet[i];
                                if (HasMachinesOfInterest)
                                    break;
                            }

                            Result = HasMachinesOfInterest;
                        }
                    }
                }
            }
            while (!Result);

            return Result;
        }

        public bool AtLastSegment()
        {
            if (IterationDirection == IterationDirection.Forwards)
            {
                return (Idx >= _Directory.SegmentDirectory.Count - 1) ||
                         (_Directory.SegmentDirectory[Idx + 1].StartTime > EndSegmentTime);
            }
            else
            {
                return (Idx <= 0) || (Directory.SegmentDirectory[Idx - 1].EndTime <= StartSegmentTime);
            }
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

            RestrictSegmentIterationBasedOnElevationRange = (minIterationElevation != Consts.NullDouble) && (MaxIterationElevation != Consts.NullDouble);
        }

        public void SetMachineRestriction(BitArray machineIDSet)
        {
            MachineIDSet = machineIDSet;
        }

        public void SegmentListExtended()
        {
            if (IterationDirection != IterationDirection.Forwards)
            {
                Log.LogCritical("Extension of segment list only valid if iterator is travelling forwards through the list");
                return;
            }

            // Reset the number of segments now expected in the segment.
            InitialNumberOfSegments = _Directory.SegmentDirectory.Count;
        }
    }
}
