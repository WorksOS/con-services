using System;
using System.Collections.Generic;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelDesignList : IList<ISiteModelDesign>, IBinaryReaderWriter
  {
    ISiteModelDesign CreateNew(string name, BoundingWorldExtent3D extents);

    int IndexOf(string designName);

    /// <summary>
    /// Indexer supporting locating designs by the design name
    /// </summary>
    ISiteModelDesign this[string designName] { get; }

    void SaveToPersistentStore(Guid projectUid, IStorageProxy storageProxy);

    void RemoveFromPersistentStore(Guid projectUid, IStorageProxy storageProxy);

    void LoadFromPersistentStore(Guid projectUid, IStorageProxy storageProxy);
  }
}
