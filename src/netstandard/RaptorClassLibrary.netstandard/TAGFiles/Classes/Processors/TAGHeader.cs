namespace VSS.TRex.TAGFiles.Classes
{
    /// <summary>
    ///  TAG files start with this initial header
    /// </summary>
    public struct TAGHeader
    {
        // Every TagFile has a header at the start the filethat looks like this
        public uint MajorVer  { get; set; }
        public uint MinorVer { get; set; }
        public uint DictionaryID { get; set; }
        public uint DictionaryMajorVer  { get; set; }
        public uint DictionaryMinorVer { get; set; }
        public uint FieldAndTypeTableOffset { get; set; } // This is a byte offset

        /// <summary>
        /// Reads the contents of the TAG file header using the provided reader
        /// </summary>
        /// <param name="reader"></param>
        public void Read(TAGReader reader)
        {
            MajorVer = reader.ReadUnSignedIntegerValue(1);
            MinorVer = reader.ReadUnSignedIntegerValue(1);
            DictionaryID = reader.ReadUnSignedIntegerValue(4);
            DictionaryMajorVer = reader.ReadUnSignedIntegerValue(1);
            DictionaryMinorVer = reader.ReadUnSignedIntegerValue(1);
            FieldAndTypeTableOffset = reader.ReadUnSignedIntegerValue(8);
        }
    }
}
