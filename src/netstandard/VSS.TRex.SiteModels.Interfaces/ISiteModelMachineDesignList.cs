using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMachineDesignList : IList<ISiteModelMachineDesign>
  {
    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    Guid DataModelID { get; set; }

    int LastIndex { get; set; }

    ISiteModelMachineDesign Locate(string name);
    ISiteModelMachineDesign Locate(int id);

    ISiteModelMachineDesign CreateNew(string name);



    /// <summary>
    /// Deserialises the list of design names using the given reader
    /// </summary>
    /// <param name="reader"></param>
    void Read(BinaryReader reader);

    /// <summary>
    /// Serialise the list of design names using the given writer
    /// </summary>
    /// <param name="writer"></param>
    void Write(BinaryWriter writer);

    void SaveToPersistentStore(IStorageProxy StorageProxy);
    void LoadFromPersistentStore();
  }
}
