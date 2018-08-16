using System;
using VSS.TRex.Common.CellPasses;

namespace VSS.TRex.Types
{
    /// <summary>
    /// Stores a paired value of GPS accuracy and tolerance (they are reported as a pair from the machine)
    /// </summary>
    [Serializable]
    public struct GPSAccuracyAndTolerance
    {
        public GPSAccuracy GPSAccuracy;
        public ushort GPSTolerance;

        public GPSAccuracyAndTolerance(GPSAccuracy gPSAccuracyValue, ushort gPSToleranceValue)
        {
            GPSAccuracy = gPSAccuracyValue;
            GPSTolerance = gPSToleranceValue;
        }

        public static GPSAccuracyAndTolerance Null()
        {
            return new GPSAccuracyAndTolerance(GPSAccuracy.Unknown, CellPassConsts.NullGPSTolerance);
        }
    }
}
