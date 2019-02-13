using System;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SiteModels
{
  public class SiteModelMetadataTests : IClassFixture<DITAGFileAndSubGridRequestsFixture>
  {
    [Fact]
    public void Test_SiteModel_Serialization_NewModel()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, 1.23);

      siteModel.SiteModelExtent.SetMaximalCoverage();
      var metaData = siteModel.MetaData;

      metaData.ID.Should().Be(guid);
      metaData.LastModifiedDate.Should().Be(siteModel.LastModifiedDate);
      metaData.CreationDate.Should().Be(siteModel.CreationDate);
      metaData.AlignmentCount.Should().Be(0);
      metaData.DesignCount.Should().Be(0);
      metaData.SiteModelExtent.Should().BeEquivalentTo(siteModel.SiteModelExtent);
      metaData.MachineCount.Should().Be(0);
      metaData.SurveyedSurfaceCount.Should().Be(0);
    }

    [Fact]
    public void Test_SiteModel_Serialization_NewModelWithSingleElements()
    {
      var guid = Guid.NewGuid();
      var siteModel = new SiteModel(guid, 1.23);

      siteModel.SiteModelExtent.SetMaximalCoverage();
      siteModel.Machines.CreateNew("Test Machine", "HardwareID", MachineType.AsphaltCompactor, 0, false, Guid.NewGuid());
      siteModel.Alignments.AddAlignmentDetails(Guid.NewGuid(), DesignDescriptor.Null(), BoundingWorldExtent3D.Null());
      siteModel.Designs.AddDesignDetails(Guid.NewGuid(), DesignDescriptor.Null(), BoundingWorldExtent3D.Null());
      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), DesignDescriptor.Null(), DateTime.UtcNow, BoundingWorldExtent3D.Null());

      var metaData = siteModel.MetaData;

      metaData.ID.Should().Be(guid);
      metaData.LastModifiedDate.Should().Be(siteModel.LastModifiedDate);
      metaData.CreationDate.Should().Be(siteModel.CreationDate);
      metaData.AlignmentCount.Should().Be(1);
      metaData.DesignCount.Should().Be(1);
      metaData.SiteModelExtent.Should().BeEquivalentTo(siteModel.SiteModelExtent);
      metaData.MachineCount.Should().Be(1);
      metaData.SurveyedSurfaceCount.Should().Be(1);
    }

  }
}
