namespace VSS.TRex.Compression
{
    /// <summary>
    /// Describes the number of records and number of bits per record stored within a bit field array
    /// </summary>
    public struct BitFieldArrayRecordsDescriptor
    {
        /// <summary>
        /// The number of records held in the vector
        /// </summary>
        public int NumRecords;

        /// <summary>
        /// The number of bits used to encode each record in the vector
        /// </summary>
        public int BitsPerRecord;
    }
}
