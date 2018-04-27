using System;
using Apache.Ignite.Core.Cache.Affinity;

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
        /// The project to process that TAG file into.
        /// This field also provides the affinity key mapping to the nodes in the mutable data grid
        /// </summary>
        [AffinityKeyMapped]
        public Guid ProjectUID;

        public Guid AssetUID;

        /// <summary>
        /// TAG File Buffer Queue key constructor taking project, asset and filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="projectUID"></param>
        /// <param name="assetUID"></param>
        public TAGFileBufferQueueKey(string fileName, Guid projectUID, Guid assetUID)
        {
            FileName = fileName;
            ProjectUID = projectUID;
            AssetUID = assetUID;
        }

        /// <summary>
        /// Provides string representation of the state of the key
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}";
    }
}
