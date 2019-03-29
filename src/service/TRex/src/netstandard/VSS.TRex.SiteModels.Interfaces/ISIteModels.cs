using System;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels.Interfaces
{
  /// <summary>
  /// Interface for the SiteModels class in TRex
  /// </summary>
  public interface ISiteModels
  {
    ISiteModel GetSiteModel(Guid ID);
    ISiteModel GetSiteModel(Guid ID, bool CreateIfNotExist);

    IStorageProxy PrimaryMutableStorageProxy { get; }
    IStorageProxy PrimaryImmutableStorageProxy { get; }

    IStorageProxy PrimaryStorageProxy(StorageMutability mutability);

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the site model to be reloaded
    /// </summary>
    void SiteModelAttributesHaveChanged(ISiteModelAttributesChangedEvent message);
  }
}
