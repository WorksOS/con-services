using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Common.Utilities.ExtensionMethods;

namespace VSS.TRex.SiteModels
{
  public class SiteProofingRunList : List<ISiteProofingRun>, ISiteProofingRunList
  {
    private const byte VERSION_NUMBER = 1;
    private const string PROOFING_RUN_LIST_STREAM_NAME = "ProofingRuns";

    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    public Guid DataModelID { get; set; }

    private ISiteProofingRun Locate(string proofingRunName, short machineID, DateTime startTime, DateTime endTime) =>
      Find(x =>
        x.Name == proofingRunName &&
        x.MachineID == machineID &&
        DateTime.Compare(x.StartTime, startTime) == 0 &&
        DateTime.Compare(x.EndTime, endTime) == 0);

    private ISiteProofingRun CreateNew(string name, short machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents)
    {
      var existingOne = Locate(name, machineID, startTime, endTime);

      if (existingOne != null)
        return existingOne;

      ISiteProofingRun proofingRun = new SiteProofingRun(name, machineID, startTime, endTime, extents);
      Add(proofingRun);

      return proofingRun;
    }

    public bool CreateAndAddProofingRun(string name, short machineID, DateTime startTime, DateTime endTime, BoundingWorldExtent3D extents)
    {
      bool result = false;
      
      if (Locate(name, machineID, startTime, endTime) == null)
        result = CreateNew(name, machineID, startTime, endTime, extents) != null;

      return result;
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
    /// proofing runs for this site model them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore(IStorageProxy storageProxy)
    {
      storageProxy.ReadStreamFromPersistentStore(DataModelID, PROOFING_RUN_LIST_STREAM_NAME, FileSystemStreamType.ProofingRuns, out MemoryStream ms);
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
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
      
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
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write((int)Count);

      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }
  }
}
