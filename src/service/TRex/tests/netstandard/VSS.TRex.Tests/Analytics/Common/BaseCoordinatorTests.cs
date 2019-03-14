using System;
using VSS.TRex.SiteModels;

namespace VSS.TRex.Tests.Analytics.Common
{
  public class BaseCoordinatorTests
  {
    protected readonly SiteModel _siteModel = new SiteModel(Guid.NewGuid());
  }
}
