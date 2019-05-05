using System;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelFactory
  {
    ISiteModel NewSiteModel(StorageMutability requiredStorageRepresentation);
    ISiteModel NewSiteModel(ISiteModel originModel, SiteModelOriginConstructionFlags originFlags);
    ISiteModel NewSiteModel_NonTransient(Guid id);
  }
}
