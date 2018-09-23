﻿using System;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.SiteModels
{
  public class SiteModelFactory : ISiteModelFactory
  {
    public ISiteModel NewSiteModel() => new SiteModel();

    public ISiteModel NewSiteModel(Guid id) => new SiteModel(id);

    public ISiteModel NewSiteModel(ISiteModel originModel, SiteModelOriginConstructionFlags originFlags) => new SiteModel(originModel, originFlags);

    public ISiteModel NewSiteModel_NonTransient(Guid id) => new SiteModel(id, false);
  }
}
