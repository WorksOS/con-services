using System;
using VSS.TRex.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Compression
{
    /// <summary>
    /// Supports logic for consistent modification of attributes used during compression using bit field arrays
    /// </summary>
    public static class AttributeValueModifiers
    {
        /// <summary>
        /// Performs a computation to modify the height into the form used by the compressed static version
        /// of the segment cell pass information, which is an integer number of millimeters above datum.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        public static int ModifiedHeight(float height) => (int)(height == Consts.NullHeight ? int.MaxValue : Math.Round(height * 1000));

        /// <summary>
        /// Performs a computation to modify the time into the form used by the compressed static version
        /// of the segment cell pass information, which is a relative time offset from an origin, expressed with 
        /// a resolution of seconds.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="timeOrigin"></param>
        /// <returns></returns>
        public static int ModifiedTime(DateTime time, DateTime timeOrigin) => (int)(Math.Floor((time - timeOrigin).TotalSeconds) + 1);

        /// <summary>
        /// Performs a computation to modify the GPS mode into the form used by the compressed static version
        /// of the segment cell pass information, which is a bit mask operation to remove other information held within the
        /// GPSMode store that may contaminate this quantity.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static int ModifiedGPSMode(GPSMode mode) => (int)mode & 0xf;
    }
}
