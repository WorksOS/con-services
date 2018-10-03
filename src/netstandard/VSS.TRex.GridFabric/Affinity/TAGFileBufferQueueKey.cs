using System;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Affinity
{
 
  /// <summary>
  /// The key used to identify TAG files in the TAG file buffer queue
  /// </summary>
  public struct TAGFileBufferQueueKey : ITAGFileBufferQueueKey
  {
        /// <summary>
        /// The name of the TAG file being processed
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The project to process that TAG file into.
        /// This field also provides the affinity key mapping to the nodes in the mutable data grid
        /// </summary>
        public Guid ProjectUID { get; set; }

        public Guid AssetID  { get; set; }

        /// <summary>
        /// TAG File Buffer Queue key constructor taking project, asset and filename
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="projectID"></param>
        /// <param name="assetID"></param>
        public TAGFileBufferQueueKey(string fileName, Guid projectID, Guid assetID)
        {
            FileName = fileName;
            ProjectUID = projectID;
            AssetID = assetID;
        }

        /// <summary>
        /// Provides string representation of the state of the key
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Project: {ProjectUID}, Asset: {AssetID}, FileName: {FileName}"; //$"Project: {ProjectUID}, Asset: {AssetUID}, FileName: {FileName}";
    }
}
