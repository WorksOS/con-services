using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Utilities.ExtensionMethods;
using VSS.TRex.Utilities.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelMachineDesignList : List<ISiteModelMachineDesign>, ISiteModelMachineDesignList, IBinaryReaderWriter
  {
    private const string kMachineDesignsListStreamName = "MachineDesigns";

    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    public Guid DataModelID { get; set; }

    // maintain an actual Index along with the name, in case items get sorted or something
    public int LastIndex { get; set; }

    public SiteModelMachineDesignList()
    {
      LastIndex = -1;
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

      ISiteModelMachineDesign Result = new SiteModelMachineDesign(LastIndex + 1, name);
      Add(Result);
      LastIndex += 1;

      return Result;
    }

    /// <summary>
    /// Serialise the list of machine using the given writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write((int) 1); //Version number

      writer.Write((int) Count);
      for (int i = 0; i < Count; i++)
        this[i].Write(writer);
    }

    public void Write(BinaryWriter writer, byte[] buffer)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Deserialises the list of machines using the given reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      int version = reader.ReadInt32();
      if (version != 1)
        throw new Exception($"Invalid version number ({version}) reading machines list, expected version (1)");

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
    /// Saves the content of the machines list into the persistent store
    /// Note: It uses a storage proxy delegate to support the TAG file ingest pipeline that creates transactional storage
    /// proxies to manage graceful rollback of changes if needed
    /// </summary>
    public void SaveToPersistentStore(IStorageProxy StorageProxy)
    {
      StorageProxy.WriteStreamToPersistentStore(DataModelID, kMachineDesignsListStreamName, FileSystemStreamType.MachineDesignNames, this.ToStream());
    }

    /// <summary>
    /// Loads the content of the machines list from the tpersistent store. If there is no item in the persistent store containing
    /// machines for this sitemodel them return an empty list.
    /// </summary>
    public void LoadFromPersistentStore()
    {
      DIContext.Obtain<ISiteModels>().StorageProxy.ReadStreamFromPersistentStore(DataModelID, kMachineDesignsListStreamName, FileSystemStreamType.MachineDesignNames, out MemoryStream MS);
      if (MS == null)
        return;

      using (MS)
      {
        this.FromStream(MS);
      }

      base.ForEach(delegate (ISiteModelMachineDesign dn)
      {
        LastIndex = dn.Id > LastIndex ? dn.Id : LastIndex;
      });

    }
  }
}
