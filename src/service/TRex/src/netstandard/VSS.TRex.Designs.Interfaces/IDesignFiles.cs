﻿using System;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.Interfaces
{
  public interface IDesignFiles
  {
    bool RemoveDesignFromCache(Guid designUid, IDesignBase design, Guid siteModelUid, bool deleteFile);

    IDesignBase Lock(Guid designUid, ISiteModelBase siteModel, double cellSize, out DesignLoadResult loadResult);

    /// <summary>
    /// Release a lock to the design referenced by the given design descriptor
    /// </summary>
    bool UnLock(Guid designUid, IDesignBase design);

    bool EnsureSufficientSpaceToLoadDesign(long designCacheSize);

    int NumDesignsInCache();

    void DesignChangedEventHandler(Guid designUid, ISiteModelBase siteModel, ImportedFileType fileType);
  }
}
