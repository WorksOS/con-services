using System;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities.ExtensionMethods;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Test_SiteModel_Creation_Default()
    {
      var siteModel = new SiteModel();

      siteModel.CreationDate.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
      siteModel.LastModifiedDate.Should().Be(siteModel.CreationDate);

      siteModel.ID.Should().Be(Guid.Empty);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidOnly_Transient()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid);

      siteModel.ID.Should().Be(guid);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidOnly_NonTransient()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, false);

      siteModel.ID.Should().Be(guid);
      siteModel.IsTransient.Should().Be(false);
    }

    [Fact]
    public void Test_SiteModel_Creation_GuidAndCellSize()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, 1.23);

      siteModel.ID.Should().Be(guid);
      siteModel.Grid.CellSize.Should().Be(1.23);
      siteModel.IsTransient.Should().Be(true);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithTransientOriginModel()
    {
      var guid = Guid.NewGuid();
      var originSiteModel = new SiteModel(guid);

      Action act = () =>
      {
        var siteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      };

      act.Should().Throw<TRexSiteModelException>().WithMessage("Cannot use a transient site model as an origin for constructing a new site model");
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_NothingPreserved()
    {
      var guid = Guid.NewGuid();
      var originSiteModel = new SiteModel(guid, false);
      var originModelModifiedDate = originSiteModel.LastModifiedDate;
      var originModelCreationDate = originSiteModel.CreationDate;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);

      newSiteModel.Should().NotBe(originSiteModel);
      newSiteModel.ID.Should().Be(guid);
      newSiteModel.IsTransient.Should().Be(false);
      newSiteModel.LastModifiedDate.Should().Be(originModelModifiedDate);
      newSiteModel.CreationDate.Should().Be(originModelCreationDate);

      newSiteModel.Grid.Should().NotBe(originSiteModel.Grid);
      newSiteModel.Grid.CellSize.Should().Be(originSiteModel.Grid.CellSize);
    }

    [Fact]
    public void Test_SiteModel_Serialization()
    {
      const int expectedSiteModelSerializedStreamSize = 96;

      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, 1.23);

      var stream = siteModel.ToStream();
      stream.Length.Should().Be(expectedSiteModelSerializedStreamSize);

      stream.Position = 0;

      var siteModel2 = new SiteModel();
      siteModel2.FromStream(stream);

      siteModel2.ID.Should().Be(siteModel.ID);
      siteModel2.Grid.ID.Should().Be(siteModel.Grid.ID);
      siteModel2.Grid.CellSize.Should().Be(siteModel.Grid.CellSize);
      siteModel2.CreationDate.Should().Be(siteModel.CreationDate);
      siteModel2.LastModifiedDate.Should().Be(siteModel.LastModifiedDate);
      siteModel2.IsTransient.Should().Be(siteModel.IsTransient);
      siteModel2.SiteModelExtent.Should().BeEquivalentTo(siteModel.SiteModelExtent);
    }
  }
}
