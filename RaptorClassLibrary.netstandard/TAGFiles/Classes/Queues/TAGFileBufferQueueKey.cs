using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.TRex.TAGFiles.Classes.Queues
{
    /// <summary>
    /// The key used to identityf TAG files in the TAG file buffer queue
    /// </summary>
    public struct TAGFileBufferQueueKey
    {
        /// <summary>
        /// The name of the TAG file being processed
        /// </summary>
        public string FileName;

        /// <summary>
        /// The project to process that TAG file into 
        /// </summary>
        public Guid ProjectUID;

        public TAGFileBufferQueueKey(string fileName, Guid projectUID)
        {
            FileName = fileName;
            ProjectUID = projectUID;
        }
    }
}
