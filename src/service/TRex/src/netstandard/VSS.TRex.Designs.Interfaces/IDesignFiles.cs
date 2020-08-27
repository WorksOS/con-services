using System;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignFiles
  {
    bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile);

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design UID
    /// </summary>
    IDesignBase Lock(Guid designUid, Guid dataModelId, double cellSize, out DesignLoadResult loadResult);

    IDesignBase Lock(Guid designUid, ISiteModelBase siteModel, double cellSize, out DesignLoadResult loadResult);

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    bool UnLock(Guid designUid, IDesignBase design);

    bool EnsureSufficientSpaceToLoadDesign(long designCacheSize);

    int NumDesignsInCache();
  }
}
