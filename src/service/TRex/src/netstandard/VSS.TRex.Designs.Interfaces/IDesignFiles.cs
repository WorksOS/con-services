using System;
using VSS.TRex.Designs.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignFiles
  {
    bool RemoveDesignFromCache(DesignDescriptor designDescriptor, IDesignBase design, bool deleteFile);
    void AddDesignToCache(DesignDescriptor designDescriptor, IDesignBase design);

    /// <summary>
    /// Acquire a lock and reference to the design referenced by the given design descriptor
    /// </summary>
    /// <param name="designDescriptor"></param>
    /// <param name="DataModelID"></param>
    /// <param name="ACellSize"></param>
    /// <param name="LoadResult"></param>
    /// <returns></returns>
    IDesignBase Lock(DesignDescriptor designDescriptor,
      Guid DataModelID, double ACellSize, out DesignLoadResult LoadResult);

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    /// <param name="designDescriptor"></param>
    /// <param name="design"></param>
    /// <returns></returns>
    bool UnLock(DesignDescriptor designDescriptor, IDesignBase design);
  }
}
