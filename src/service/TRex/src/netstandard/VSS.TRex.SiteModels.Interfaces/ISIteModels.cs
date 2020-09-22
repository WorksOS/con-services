using System;
using System.Collections.Generic;
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
    ISiteModel GetSiteModel(Guid id);
    ISiteModel GetSiteModel(Guid id, bool createIfNotExist);

    /// <summary>
    /// Retrieves a site model from persistent storage with no reference to cached
    /// site models, or attributes of the site model that might otherwise affect its
    /// retrieval, such as being marked for deletion
    /// </summary>
    ISiteModel GetSiteModelRaw(Guid id);

    void DropSiteModel(Guid id);

    StorageMutability PrimaryMutability { get; }

    IStorageProxy PrimaryMutableStorageProxy { get; }
    IStorageProxy PrimaryImmutableStorageProxy { get; }

    IStorageProxy PrimaryStorageProxy(StorageMutability mutability);

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the site model to be reloaded
    /// </summary>
    void SiteModelAttributesHaveChanged(ISiteModelAttributesChangedEvent message);

    List<ISiteModel> GetSiteModels();
  }
}
