using System;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignFiles
  {
    bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile);

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design UID
    /// </summary>
    /// <param name="designUid"></param>
    /// <param name="dataModelId"></param>
    /// <param name="cellSize"></param>
    /// <param name="loadResult"></param>
    /// <returns></returns>
    IDesignBase Lock(Guid designUid, 
      Guid dataModelId, double cellSize, out DesignLoadResult loadResult);

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    /// <param name="designUid"></param>
    /// <param name="design"></param>
    /// <returns></returns>
    bool UnLock(Guid designUid, IDesignBase design);
  }
}
