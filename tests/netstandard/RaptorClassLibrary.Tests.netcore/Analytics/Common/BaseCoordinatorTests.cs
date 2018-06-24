using System;
using VSS.TRex.SiteModels;

namespace VSS.TRex.Tests.netcore.Analytics.Common
{
  public class BaseCoordinatorTests : BaseTests
  {
    protected readonly SiteModel _siteModel = new SiteModel(Guid.NewGuid());
  }
}
