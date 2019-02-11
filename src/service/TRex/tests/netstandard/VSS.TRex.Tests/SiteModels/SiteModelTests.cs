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
        var _ = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
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
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveExistenceMap()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.ExistenceMap;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveExistenceMap);
      newSiteModel.ExistenceMap.Should().Be(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveExistenceMap()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.ExistenceMap;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.ExistenceMap.Should().NotBe(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveGrid()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Grid;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveGrid);
      newSiteModel.Grid.Should().Be(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveGrid()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.ExistenceMap;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.ExistenceMap.Should().NotBe(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveCSIB()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.CSIB();

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveCsib);
      newSiteModel.CSIB().Should().Be(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveCSIB()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.CSIB();

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.ExistenceMap.Should().NotBe(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveDesigns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Designs;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveDesigns);
      newSiteModel.Designs.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveDesigns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Designs;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.Designs.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveSurveyedSurfaces()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SurveyedSurfaces;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveSurveyedSurfaces);
      newSiteModel.SurveyedSurfaces.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveSurveyedSurfaces()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SurveyedSurfaces;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.SurveyedSurfaces.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveMachines()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Machines;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveMachines);
      newSiteModel.Machines.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveMachines()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Machines;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.Machines.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveMachineTargetValues()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.MachinesTargetValues;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveMachineTargetValues);
      newSiteModel.MachinesTargetValues.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveMachineTargetValues()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.MachinesTargetValues;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.MachinesTargetValues.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveMachineDesigns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SiteModelMachineDesigns;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveMachineDesigns);
      newSiteModel.SiteModelMachineDesigns.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveMachineDesigns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SiteModelMachineDesigns;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.SiteModelMachineDesigns.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveProofingRuns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SiteProofingRuns;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveProofingRuns);
      newSiteModel.SiteProofingRuns.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveProofingRuns()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.SiteProofingRuns;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.SiteProofingRuns.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_PreserveAlignments()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Alignments;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveAlignments);
      newSiteModel.Alignments.Should().BeSameAs(original);
    }

    [Fact]
    public void Test_SiteModel_Creation_WithNonTransientOriginModel_DoNotPreserveAlignments()
    {
      var originSiteModel = new SiteModel(Guid.NewGuid(), false);
      var original = originSiteModel.Alignments;

      var newSiteModel = new SiteModel(originSiteModel, SiteModelOriginConstructionFlags.PreserveNothing);
      newSiteModel.Alignments.Should().NotBeSameAs(original);
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
