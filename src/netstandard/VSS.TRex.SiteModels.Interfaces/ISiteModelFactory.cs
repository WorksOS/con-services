using System;

namespace VSS.TRex.SiteModels.Interfaces
{
  public interface ISiteModelFactory
  {
    ISiteModel NewSiteModel();
    ISiteModel NewSiteModel(Guid id);
  }
}
