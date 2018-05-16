using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// Represents the state of a TAG file stored in the TAG file buffer queue awaiting processing.
    /// </summary>
    [Serializable]
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
        [AffinityKeyMapped]
        public Guid ProjectID;

        /// <summary>
        /// UID identifier of the asset to process this TAG file into
        /// </summary>
        public Guid AssetID;
    }
}
