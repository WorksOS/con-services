using System;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Analytics.Common
{
  public class BaseCoordinatorTests : IClassFixture<DITagFileFixture>
  {
    protected readonly SiteModel _siteModel;

    public BaseCoordinatorTests()
    {
      _siteModel = new SiteModel(Guid.NewGuid());
      _siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);
    }
  }
}
