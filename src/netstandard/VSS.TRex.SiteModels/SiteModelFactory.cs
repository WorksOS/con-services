using System;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelFactory : ISiteModelFactory
  {
    public ISiteModel NewSiteModel() => new SiteModel();
    public ISiteModel NewSiteModel(Guid id) => new SiteModel(id);
  }
}
