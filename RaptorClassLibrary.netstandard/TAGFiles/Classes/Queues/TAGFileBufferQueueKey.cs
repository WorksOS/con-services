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
        public long ProjectID;
        //public Guid ProjectUID;

        //public long AssetID;
        public Guid AssetID;

        /// <summary>
        /// TAG File Buffer Queue key constructor taking project, asset and filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="projectID"></param>
        /// <param name="assetID"></param>
        public TAGFileBufferQueueKey(string fileName, long projectID, Guid assetID /*Guid projectUID */)
        {
            FileName = fileName;
            //ProjectUID = projectUID;
            //AssetUID = assetUID;
            ProjectID = projectID;
            AssetID = assetID;
        }

        /// <summary>
        /// Provides string representation of the state of the key
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Project: {ProjectID}, Asset: {AssetID}, FileName: {FileName}"; //$"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}";
    }
}
