using System;
using System.Collections.Generic;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Common.Utilities.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelMachineDesignList : IList<ISiteModelMachineDesign>, IBinaryReaderWriter
  {
    /// <summary>
    /// The identifier of the site model owning this list of machine design names
    /// </summary>
    Guid DataModelID { get; set; }

    ISiteModelMachineDesign Locate(string name);
    ISiteModelMachineDesign Locate(int id);

    ISiteModelMachineDesign CreateNew(string name);
    
    void SaveToPersistentStore(IStorageProxy storageProxy);
    void LoadFromPersistentStore();
  }
}
