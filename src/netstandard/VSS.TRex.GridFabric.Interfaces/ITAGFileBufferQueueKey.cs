using System;

namespace VSS.TRex.GridFabric.Interfaces
{
  public interface ITAGFileBufferQueueKey : IProjectAffinity
  {
    /// <summary>
    /// The name of the TAG file being processed
    /// </summary>
    string FileName { get; set; }

    Guid AssetID { get; set; }
  }
}
