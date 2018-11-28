using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteProofingRunList : IList<ISiteProofingRun>, IBinaryReaderWriter
  {
    /// <summary>
    /// The identifier of the site model owning this list of proofing runs.
    /// </summary>
    Guid DataModelID { get; set; }

    ISiteProofingRun Locate(string proofingRunName, long machineID, DateTime startTime, DateTime endTime);

    ISiteProofingRun CreateNew(string name, long machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents);

    void SaveToPersistentStore(IStorageProxy storageProxy);
    void LoadFromPersistentStore();

    //int IndexOf(string proofingRunName, long machineID, DateTime startTime, DateTime endTime);

    ///// <summary>
    ///// Indexer supporting locating proofing runs by the proofing run name
    ///// </summary>
    ///// <param name="proofingRunName"></param>
    ///// <returns></returns>
    //ISiteProofingRun this[string proofingRunName, long machineID, DateTime startTime, DateTime endTime] { get; }
  }
}
