namespace VSS.TRex.TAGFiles.Types
{
    /// <summary>
    /// Denotes a set of error conditions that the ReadFile method in TagFile can return.
    /// </summary>
    public enum TAGReadResult
    {
        /// <summary>
        /// Everything is OK
        /// </summary>
        NoError,

        /// <summary>
        /// Processing was terminated for an unknwon reason
        /// </summary>
        ProcessingTerminated,

        /// <summary>
        /// TAG dictionary inthe file was invalid, corrupted or truncated
        /// </summary>
        InvalidDictionary,

        /// <summary>
        /// Unknwon value type id in a TAG
        /// </summary>
        InvalidValueTypeID,

        /// <summary>
        /// A binary TAG value in the file had an invalid size
        /// </summary>
        InvalidBinaryDataSize,

        /// <summary>
        /// Faied to initialise TAG value sink in preparation for processing
        /// </summary>
        SinkStartingFailure,

        /// <summary>
        /// TAG value sink failed to complete processing of TAG values
        /// </summary>
        SinkFinishingFailure,

        /// <summary>
        /// Given file does not exist
        /// </summary>
        FileDoesNotExist,

        // Unable to open the given file
        CouldNotOpenFile,

        /// <summary>
        /// Given TAG file is zero length in size
        /// </summary>
        ZeroLengthFile,

        /// <summary>
        /// An unknwon TAG value was encountered in the file
        /// </summary>
        InvalidValue
    }
}
