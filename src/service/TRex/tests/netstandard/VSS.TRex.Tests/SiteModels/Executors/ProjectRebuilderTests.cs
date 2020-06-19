using System;
using FluentAssertions;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilder(Guid.NewGuid());
      rebuilder.Should().NotBeNull();
    }

    [Fact]
    public void ValidateNoAciveRebuilderForProject()
    {
      // Create sitemodel, add project metadata for it, check validator hates it...
      var sitemodel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var metaData = new RebuildSiteModelMetaData()
      {
        ProjectUid = sitemodel.ID
      };


      Assert.True(false);
    }
  }
}
