using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// Provides description of GPS accuracy for the current cell, current machine and current datetime
    /// </summary>
    public enum GPSAccuracy
    {
        /// <summary>
        /// Fine accuracy
        /// </summary>
        Fine = 0,
        /// <summary>
        /// Medium accuracy
        /// </summary>
        Medium = 1,
        /// <summary>
        /// Coarse accuracy
        /// </summary>
        Coarse = 2,
        Unknown = 3
    }
}
