using System;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.SiteModels.Interfaces
{
  /// <summary>
  /// Interface for the SiteModels class in TRex
  /// </summary>
  // public interface ISiteModels
  public interface ISiteModelsImmutable : ISiteModels
  {
  }

  public interface ISiteModels
  {
    ISiteModel GetSiteModel(Guid ID);
    ISiteModel GetSiteModel(Guid ID, bool CreateIfNotExist);
    ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID);
    ISiteModel GetSiteModel(IStorageProxy storageProxy, Guid ID, bool CreateIfNotExist);

    IStorageProxy StorageProxy { get; }

    /// <summary>
    /// Handles the situation when TAG file processing or some other activity has modified the attributes of a site model
    /// requiring the sitemodel to be reloaded
    /// </summary>
    /// <param name="SiteModelID"></param>
    void SiteModelAttributesHaveChanged(Guid SiteModelID, ISiteModelAttributesChangedEvent message);
  }
}
