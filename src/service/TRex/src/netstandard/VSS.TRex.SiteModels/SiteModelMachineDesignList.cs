using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMachineDesignList : List<ISiteModelMachineDesign>, ISiteModelMachineDesignList
  {
    private const string MACHINE_DESIGN_LIST_STREAM_NAME = "MachineDesigns";

    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    public Guid DataModelID { get; set; }

    // maintain an actual Index along with the name, in case items get sorted or something
    private int GetLastId()
    {
      return this.Count > 0 ? this.Max(x => x.Id) : -1;
    }

    /// <summary>
    /// Finds the machine design in the list whose name matches the given name
    /// It returns NIL if there is no matching design name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ISiteModelMachineDesign Locate(string name) => Find(x => name.Equals(x.Name));

    public ISiteModelMachineDesign Locate(int id) => Find(x => id.Equals(x.Id));

    public ISiteModelMachineDesign CreateNew(string name)
    {
      var existingOne = Locate(name);

      if (existingOne != null)
      {
        return existingOne;
      }

      ISiteModelMachineDesign result = new SiteModelMachineDesign(GetLastId() + 1, name);
      Add(result);

      return result;
    }

    /// <summary>
    /// Serialise the list of machine designs using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(UtilitiesConsts.ReaderWriterVersion); 

      writer.Write((int) Count);
      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Deserialises the list of machine designs using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != UtilitiesConsts.ReaderWriterVersion)
        throw new Exception($"Invalid version number ({version}) reading machine designs list, expected version (1)");

      int count = reader.ReadInt32();
      Capacity = count;

      for (int i = 0; i < count; i++)
      {
        SiteModelMachineDesign siteModelMachineDesign = new SiteModelMachineDesign();
        siteModelMachineDesign.Read(reader);
        Add(siteModelMachineDesign);
      }
    }

    /// <summary>
    /// Saves the content of the machine designs list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(IStorageProxy storageProxy)
    {
      storageProxy.WriteStreamToPersistentStore(DataModelID, MACHINE_DESIGN_LIST_STREAM_NAME, FileSystemStreamType.MachineDesignNames, this.ToStream(), this);
    }
    
    /// <summary>
    /// Loads the content of the machine designs list from the persistent store. If there is no item in the persistent store containing
    /// machine designs for this sitemodel them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore()
    {
      DIContext.Obtain<ISiteModels>().StorageProxy.ReadStreamFromPersistentStore(DataModelID, MACHINE_DESIGN_LIST_STREAM_NAME, FileSystemStreamType.MachineDesignNames, out MemoryStream MS);
      if (MS == null)
        return;

      using (MS)
      {
        this.FromStream(MS);
      }
    }
  }
}
