using System;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels
{
  public class SiteModelFactory : ISiteModelFactory
  {
    public ISiteModel NewSiteModel(StorageMutability requiredStorageRepresentation)
    {
      var siteModel = new SiteModel();
      siteModel.SetStorageRepresentationToSupply(requiredStorageRepresentation);

      return siteModel;
    }

    public ISiteModel NewSiteModel(ISiteModel originModel, SiteModelOriginConstructionFlags originFlags) => new SiteModel(originModel, originFlags);

    public ISiteModel NewSiteModel_NonTransient(Guid id) => new SiteModel(id, false);
  }
}
