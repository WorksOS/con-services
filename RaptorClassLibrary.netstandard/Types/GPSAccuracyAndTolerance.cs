using VSS.VisionLink.Raptor.Cells;

namespace VSS.VisionLink.Raptor.Types
{
    /// <summary>
    /// Stores a paired value of GPS accuracy and tolerance (they are reported as a pair from the machine)
    /// </summary>
    public struct GPSAccuracyAndTolerance
    {
        public GPSAccuracy GPSAccuracy;
        public short GPSTolerance;

        public GPSAccuracyAndTolerance(GPSAccuracy gPSAccuracyValue, short gPSToleranceValue)
        {
            GPSAccuracy = gPSAccuracyValue;
            GPSTolerance = gPSToleranceValue;
        }

        public static GPSAccuracyAndTolerance Null()
        {
            return new GPSAccuracyAndTolerance(GPSAccuracy.Unknown, CellPass.NullGPSTolerance);
        }
    }
}
