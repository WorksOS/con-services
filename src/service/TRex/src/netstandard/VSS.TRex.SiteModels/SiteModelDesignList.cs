using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  public class SiteModelDesignList : List<ISiteModelDesign>, ISiteModelDesignList
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelDesignList>();

    private const byte VERSION_NUMBER = 1;
    private const string LIST_STREAM_NAME = "SiteModelDesigns";

    /// <summary>
    /// Indexer supporting locating designs by the design name
    /// </summary>
    /// <param name="designName"></param>
    /// <returns></returns>
    public ISiteModelDesign this[string designName]
    {
      get
      {
        int index = IndexOf(designName);
        return index > 0 ? this[index] : null;
      }
    }

    public int IndexOf(string designName) => FindIndex(x => x.Name == designName);

    public ISiteModelDesign CreateNew(string name, BoundingWorldExtent3D extents)
    {
      int index = IndexOf(name);

      if (index != -1)
      {
        Log.LogError($"An identical design ({name}) already exists in the designs for this site.");
        return this[index];
      }

      ISiteModelDesign design = new SiteModelDesign(name, extents);
      Add(design);

      return design;
    }

    /// <summary>
    /// Saves the content of the proofing run list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(Guid projectUid, IStorageProxy storageProxy)
    {
      storageProxy.WriteStreamToPersistentStore(projectUid, LIST_STREAM_NAME, FileSystemStreamType.MachineDesigns, this.ToStream(), this);
    }

    /// <summary>
    /// Loads the content of the proofing run list from the persistent store. If there is no item in the persistent store containing
    /// proofing runs for this site model them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore(Guid projectUid, IStorageProxy storageProxy)
    {
      storageProxy.ReadStreamFromPersistentStore(projectUid, LIST_STREAM_NAME, FileSystemStreamType.MachineDesigns, out MemoryStream ms);
      if (ms == null)
        return;

      using (ms)
      {
        this.FromStream(ms);
      }
    }

    /// <summary>
    /// Deserializes the list of proofing runs using the given reader
    /// </summary>
    /// <param name="reader"></param>

    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      int count = reader.ReadInt32();
      Capacity = count;

      for (int i = 0; i < count; i++)
      {
        string name = reader.ReadString();
        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        extents.Read(reader);

        Add(new SiteModelDesign(name, extents));
      }
    }

    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write((int)Count);

      for (int i = 0; i < Count; i++)
      {
        writer.Write(this[i].Name);
        this[i].Extents.Write(writer);
      }
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
  }
}
