using System;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels
{
  public class SiteModelFactory : ISiteModelFactory
  {
    public ISiteModel NewSiteModel(StorageMutability requiredStorageRepresentation) => new SiteModel(requiredStorageRepresentation);

    public ISiteModel NewSiteModel(ISiteModel originModel, SiteModelOriginConstructionFlags originFlags) => new SiteModel(originModel, originFlags);

    public ISiteModel NewSiteModel_NonTransient(Guid id, StorageMutability requiredStorageRepresentation) => new SiteModel(id, requiredStorageRepresentation, false);
  }
}
