using System;
using FluentAssertions;
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

    [Fact]
    public void GetSiteModel_WithMarkedForDeletion_Suppression()
    {
      var sm = new TRex.SiteModels.SiteModels();

      var siteModelId = Guid.NewGuid();
      var siteModel = sm.GetSiteModel(siteModelId, true);

      siteModel.MarkForDeletion();

      sm.GetSiteModel(siteModelId, false).Should().BeNull("Site models marked for deletion cannot be returned for standard query operations");
    }

    [Fact]
    public void GetSiteModel_WithMarkedForDeletion_FlagRemoval()
    {
      var sm = new TRex.SiteModels.SiteModels();

      var siteModelId = Guid.NewGuid();
      var siteModel = sm.GetSiteModel(siteModelId, true);

      siteModel.MarkForDeletion();
      sm.GetSiteModel(siteModelId, false).Should().BeNull("Site models marked for deletion cannot be returned for standard query operations");

      siteModel.RemovedMarkForDeletion();
      sm.GetSiteModel(siteModelId, false).Should().NotBeNull("Site models marked for deletion can be returned to active duty after partial delete operations");
    }

    [Fact]
    public void GetSiteModelRaw_WithoutMarkedForDeletion()
    {
      var sm = new TRex.SiteModels.SiteModels();

      var siteModelId = Guid.NewGuid();
      var siteModel = sm.GetSiteModel(siteModelId, true);
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy);

      var queriedSiteModel = sm.GetSiteModelRaw(siteModelId);
      queriedSiteModel.Should().NotBeNull();
      queriedSiteModel.ID.Should().Be(siteModelId);
    }

    [Fact]
    public void GetSiteModelRaw_WithMarkedForDeletion()
    {
      var sm = new TRex.SiteModels.SiteModels();

      var siteModelId = Guid.NewGuid();
      var siteModel = sm.GetSiteModel(siteModelId, true);

      siteModel.MarkForDeletion();

      var queriedSiteModel = sm.GetSiteModelRaw(siteModelId);
      queriedSiteModel.Should().NotBeNull();
      queriedSiteModel.ID.Should().Be(siteModelId);
    }
  }
}
