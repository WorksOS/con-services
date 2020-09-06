using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SiteModels
{
  public class SiteModelDesignList : List<ISiteModelDesign>, ISiteModelDesignList
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SiteModelDesignList>();

    private const byte VERSION_NUMBER = 1;
    public const string LIST_STREAM_NAME = "SiteModelDesigns";

    /// <summary>
    /// Indexer supporting locating designs by the design name
    /// </summary>
    public ISiteModelDesign this[string designName]
    {
      get
      {
        var index = IndexOf(designName);
        return index > 0 ? this[index] : null;
      }
    }

    public int IndexOf(string designName) => FindIndex(x => x.Name == designName);

    public ISiteModelDesign CreateNew(string name, BoundingWorldExtent3D extents)
    {
      var index = IndexOf(name);

      if (index != -1)
      {
        _log.LogError($"An identical design ({name}) already exists in the designs for this site.");
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
      using var stream = this.ToStream();
      storageProxy.WriteStreamToPersistentStore(projectUid, LIST_STREAM_NAME, FileSystemStreamType.MachineDesigns, stream, this);
    }

    /// <summary>
    /// Removes the content of the proofing run list from the persistent store
    /// </summary>
    public void RemoveFromPersistentStore(Guid projectUid, IStorageProxy storageProxy)
    {
      RemoveFromPersistentStoreStatic(projectUid, storageProxy);
    }

    /// <summary>
    /// Removes the content of the proofing run list from the persistent store
    /// </summary>
    public static void RemoveFromPersistentStoreStatic(Guid projectUid, IStorageProxy storageProxy)
    {
       storageProxy.RemoveStreamFromPersistentStore(projectUid, FileSystemStreamType.MachineDesigns, LIST_STREAM_NAME);
    }

    /// <summary>
    /// Loads the content of the proofing run list from the persistent store. If there is no item in the persistent store containing
    /// proofing runs for this site model them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore(Guid projectUid, IStorageProxy storageProxy)
    {
      storageProxy.ReadStreamFromPersistentStore(projectUid, LIST_STREAM_NAME, FileSystemStreamType.MachineDesigns, out var ms);
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

    public void Read(BinaryReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        var count = reader.ReadInt32();
        Capacity = count;

        for (var i = 0; i < count; i++)
        {
          var name = reader.ReadString();
          var extents = new BoundingWorldExtent3D();
          extents.Read(reader);

          Add(new SiteModelDesign(name, extents));
        }
      }
    }

    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(Count);

      for (var i = 0; i < Count; i++)
      {
        writer.Write(this[i].Name);
        this[i].Extents.Write(writer);
      }
    }
  }
}
