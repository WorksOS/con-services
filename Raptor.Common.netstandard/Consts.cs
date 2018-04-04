namespace VSS.VisionLink.Raptor.Common
{
    public static class Consts
    {
        /// <summary>
        /// IEEE single/float null value
        /// </summary>
        public const float NullSingle = 3.4E38f;

        /// <summary>
        /// IEEE single/float null value
        /// </summary>
        public const float NullFloat = 3.4E38f;

        /// <summary>
        /// IEEE double null value
        /// </summary>
        public const double NullDouble = 1E308;

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
