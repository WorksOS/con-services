using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Common
{
    public class Consts
    {
        /// <summary>
        /// IEEE single/float null value
        /// </summary>
        public const float NullSingle = (float)3.4E38;

        /// <summary>
        /// IEEE single/float null value
        /// </summary>
        public const float NullFloat = (float)3.4E38;

        /// <summary>
        /// IEEE double null value
        /// </summary>
        public const double NullDouble = (float)1E308;

        /// <summary>
        /// Value representing a null height encoded as an IEEE single
        /// </summary>
        public const float NullHeight = NullSingle;

        /// <summary>
        /// Null ID for a design reference descriptor ID
        /// </summary>
        public const int kNoDesignNameID = 0;

        /// <summary>
        /// ID representing any design ID in a filter
        /// </summary>
        public const int kAllDesignsNameID = -1;

        /// <summary>
        /// Largest GPS accuracy error value
        /// </summary>
        public const short kMaxGPSAccuracyErrorLimit = 0x3FFF;
    }
}
