using System;

namespace VSS.TRex.TAGFiles.Models
{
    /// <summary>
    /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
    /// </summary>
    public class TAGFileBufferQueueItem
    {
        /// <summary>
        /// The date at which the TAG file was inserted into the buffer queue. This field is indexed to permit
        /// processing TAG files in the order they arrived
        /// </summary>
        public DateTime InsertUTC;

        /// <summary>
        /// The original filename for the TAG file
        /// </summary>
        public string FileName;

        /// <summary>
        /// The contents of the TAG file, as a byte array
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// UID identifier of the project to process this TAG file into.
        /// This field is used as the affinity key map that determines which mutable server will
        /// store this TAG file.
        /// </summary>
        public Guid ProjectID;

        /// <summary>
        /// UID identifier of the asset to process this TAG file into
        /// </summary>
        public Guid AssetID;

        /// <summary>
        ///   Is machine a JohnDoe. No telematic device on board to identify machine or No AssetID in system
        ///   JohnDoe machine are assigned a unique Guid
        /// </summary>
        public bool IsJohnDoe;
    }
}
