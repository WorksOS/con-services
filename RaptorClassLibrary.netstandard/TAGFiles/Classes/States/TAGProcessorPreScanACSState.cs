using System.Collections.Generic;
using VSS.TRex.Geometry;

namespace VSS.TRex.TAGFiles.Classes.States
{
    /// <summary>
    /// Handles scanning a TAG value stream and extracting the blade/track/wheel/read positions into a set
    /// of groups to permit recalculation of coordinates between coordinate systems
    /// </summary>
    public class TAGProcessorPreScanACSState : TAGProcessorStateBase
    {
        public List<UTMCoordPointPair> BladePositions { get; set; } = new List<UTMCoordPointPair>();
        public List<UTMCoordPointPair> RearAxlePositions { get; set; } = new List<UTMCoordPointPair>();
        public List<UTMCoordPointPair> TrackPositions { get; set; } = new List<UTMCoordPointPair>();
        public List<UTMCoordPointPair> WheelPositions { get; set; } = new List<UTMCoordPointPair>();

        public TAGProcessorPreScanACSState()
        {

        }

        public override bool ProcessEpochContext()
        {
            BladePositions.Add(new UTMCoordPointPair(DataLeft, DataRight, UTMZone));

            RearAxlePositions.Add(new UTMCoordPointPair(DataRearLeft, DataRearRight, UTMZone));

            TrackPositions.Add(new UTMCoordPointPair(DataTrackLeft, DataTrackRight, UTMZone));

            WheelPositions.Add(new UTMCoordPointPair(DataWheelLeft, DataWheelRight, UTMZone));

            ProcessedEpochCount++;

            return true; // Force reading of entire TAG file contents
        }
    }
}
