using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelsTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Creation()
    {
      var sm = new TRex.SiteModels.SiteModels();

      sm.Should().NotBeNull();
    }

    [Fact]
    public void GetSiteModels()
    {
      var sm = new TRex.SiteModels.SiteModels();

      sm.GetSiteModels().Should().NotBeNull();
      sm.GetSiteModels().Count.Should().Be(0);

      var siteModel = sm.GetSiteModel(Guid.NewGuid(), true);
      sm.GetSiteModels().Should().NotBeNull();
      sm.GetSiteModels().Count.Should().Be(1);
      sm.GetSiteModels()[0].Should().BeEquivalentTo(siteModel);
    }
  }
}
