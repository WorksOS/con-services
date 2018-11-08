using System;

namespace VSS.TRex.Utilities
{
    public static class Formatters
    {
        /// <summary>
        /// Formats a cell pass time value in a culture invariant format. If offset is true the formatted time will
        /// be converted to the GMT offset in the current local
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="offset"></param>
        public static string FormatCellPassTime(DateTime dateTime, bool offset = true)
        {
            return $"{(offset ? dateTime + Time.GPS.GetLocalGMTOffset() : dateTime):yyyy/MMM/dd HH:mm:ss.zzz}";
        }
    }
}
