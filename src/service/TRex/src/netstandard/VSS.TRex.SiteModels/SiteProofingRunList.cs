using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteProofingRunList : List<ISiteProofingRun>, ISiteProofingRunList
  {
    private const string PROOFING_RUN_LIST_STREAM_NAME = "ProofingRuns";

    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    public Guid DataModelID { get; set; }

    //private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    ///// <summary>
    ///// Indexer supporting locating proofing runs by the proofing run name
    ///// </summary>
    ///// <param name="proofingRunName"></param>
    ///// <param name="machineID"></param>
    ///// <param name="startTime"></param>
    ///// <param name="endTime"></param>
    ///// <returns></returns>
    //public ISiteProofingRun this[string proofingRunName, long machineID, DateTime startTime, DateTime endTime]
    //{
    //  get
    //  {
    //    int index = IndexOf(proofingRunName, machineID, startTime, endTime);
    //    return index > 0 ? this[index] : null;
    //  }
    //}

    public ISiteProofingRun Locate(string proofingRunName, long machineID, DateTime startTime, DateTime endTime) =>
      Find(x =>
        x.Name == proofingRunName &&
        x.MachineID == machineID &&
        DateTime.Compare(x.StartTime, startTime) == 0 &&
        DateTime.Compare(x.EndTime, endTime) == 0);

    public ISiteProofingRun CreateNew(string name, long machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents)
    {
      var existingOne = Locate(name, machineID, startTime, endTime);

      if (existingOne != null)
        return existingOne;

      ISiteProofingRun proofingRun = new SiteProofingRun(name, machineID, startTime, endTime, extents);
      Add(proofingRun);

      return proofingRun;
    }

    public void SaveToPersistentStore(IStorageProxy storageProxy)
    {
      throw new NotImplementedException();
    }

    public void LoadFromPersistentStore()
    {
      throw new NotImplementedException();
    }

    //  public int IndexOf(string proofingRunName, long machineID, DateTime startTime, DateTime endTime) => 
    //    FindIndex(x => 
    //      x.Name == proofingRunName && 
    //      x.MachineID == machineID && 
    //      DateTime.Compare(x.StartTime, startTime) == 0 && 
    //      DateTime.Compare(x.EndTime, endTime) == 0);
    //}
    public void Read(BinaryReader reader)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer, byte[] buffer)
    {
      throw new NotImplementedException();
    }
  }
}
