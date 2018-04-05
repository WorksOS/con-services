namespace VSS.VisionLink.Raptor.TAGFiles.Types
{
    /// <summary>
    /// The types of TAGs (fields) supported in the TAG file. Field data types that may be found in the tag file
    /// </summary>
    public enum TAGDataType
    {
        /// <summary>
        /// Signed 4 bit integer
        /// </summary>
        t4bitInt,

        /// <summary>
        /// Unsigned 4 bit integer
        /// </summary>
        t4bitUInt,

        /// <summary>
        /// Signed 8 bit integer
        /// </summary>
        t8bitInt,

        /// <summary>
        /// Unsigned 8 bit integer
        /// </summary>
        t8bitUInt,

        /// <summary>
        /// Signed 12 bit integer
        /// </summary>
        t12bitInt,

        /// <summary>
        /// Unsigned 12 bit integer
        /// </summary>
        t12bitUInt,

        /// <summary>
        /// Signed 16 bit integer
        /// </summary>
        t16bitInt,

        /// <summary>
        /// Unsigned 16 bit integer
        /// </summary>
        t16bitUInt,

        /// <summary>
        /// Signed 32 bit integer
        /// </summary>
        t32bitInt,

        /// <summary>
        /// Unsigned 32 bit integer
        /// </summary>
        t32bitUInt,

        /// <summary>
        /// 4 byte IEEE single precision floating point number
        /// </summary>
        tIEEESingle,

        /// <summary>
        ///  8 byte IEEE double precision floating point number,
        /// </summary>
        tIEEEDouble,

        /// <summary>
        /// ANSI string
        /// </summary>
        tANSIString,

        /// <summary>
        /// Unicode string
        /// </summary>
        tUnicodeString,

        /// <summary>
        /// An explicitly empty TAG (no value)
        /// </summary>
        tEmptyType
    }
}
