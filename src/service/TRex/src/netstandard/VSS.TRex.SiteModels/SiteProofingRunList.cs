using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.SiteModels
{
  public class SiteProofingRunList : List<ISiteProofingRun>, ISiteProofingRunList
  {
    private const string PROOFING_RUN_LIST_STREAM_NAME = "ProofingRuns";

    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    public Guid DataModelID { get; set; }

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

    /// <summary>
    /// Saves the content of the proofing run list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(IStorageProxy storageProxy)
    {
      storageProxy.WriteStreamToPersistentStore(DataModelID, PROOFING_RUN_LIST_STREAM_NAME, FileSystemStreamType.ProofingRuns, this.ToStream(), this);
    }

    /// <summary>
    /// Loads the content of the proofing run list from the persistent store. If there is no item in the persistent store containing
    /// proofing runs for this sitemodel them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore()
    {
      DIContext.Obtain<ISiteModels>().StorageProxy.ReadStreamFromPersistentStore(DataModelID, PROOFING_RUN_LIST_STREAM_NAME, FileSystemStreamType.ProofingRuns, out MemoryStream ms);
      if (ms == null)
        return;

      using (ms)
      {
        this.FromStream(ms);
      }
    }

    /// <summary>
    /// Deserialises the list of proofing runs using the given reader
    /// </summary>
    /// <param name="reader"></param>

    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != UtilitiesConsts.ReaderWriterVersion)
        throw new Exception($"Invalid version number ({version}) reading proofing run list, expected version (1)");

      int count = reader.ReadInt32();
      Capacity = count;

      for (int i = 0; i < count; i++)
      {
        SiteProofingRun siteProofingRun = new SiteProofingRun();
        siteProofingRun.Read(reader);
        Add(siteProofingRun);
      }
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(UtilitiesConsts.ReaderWriterVersion);

      writer.Write((int)Count);

      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
  }
}
