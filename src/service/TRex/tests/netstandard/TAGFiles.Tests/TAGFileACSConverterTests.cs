using System;
using CoreX.Interfaces;
using CoreX.Wrapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileACSConverterTests : IClassFixture<DITagFileFixture>
  {
    /// <summary>
    ///  The real deal test setup for ACS conversion
    /// </summary>
    private void InjectACSDependencies()
    {
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<IACSTranslator, ACSTranslator>())
        .Add(x => x.AddSingleton<ICoreXWrapper, CoreXWrapper>())
        .Complete();
    }

    private ISiteModel BuildModel()
    {
      // Create the site model and machine etc to aggregate the processed TAG file into
      var targetSiteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);

      // Switch to mutable storage representation to allow creation of content in the site model
      targetSiteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      return targetSiteModel;
    }

    public TAGFileACSConverterTests()
    {
      InjectACSDependencies();
    }

    [Fact()]
    public void Test_ACS_Coordinate_Conversion_Fail_No_ProjectCSIB()
    {
      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false);
      converter.IsUTMCoordinateSystem.Should().BeTrue();
      converter.ReadResult.Should().Be(TAGReadResult.CoordinateConversionFailure);
      converter.ProcessedCellPassCount.Should().Be(0);
      converter.ProcessedEpochCount.Should().Be(0);
    }


    [Fact()]
    public void Test_ACS_Coordinate_Conversion()
    {
      var targetSiteModel = BuildModel();
      DITAGFileAndSubGridRequestsWithIgniteFixture.AddCSIBToSiteModel(ref targetSiteModel, TestCommonConsts.DIMENSIONS_2012_DC_CSIB);

      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile.tag", Guid.NewGuid(), false, ref targetSiteModel);
      Assert.True(converter.IsUTMCoordinateSystem, "Tagfile should be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedBladePositions[0].Left.X.Should().Be(537675.68172357627);
      converter.Processor.ConvertedBladePositions[0].Left.Y.Should().Be(5427402.34152853);
      converter.Processor.ConvertedBladePositions[0].Right.X.Should().Be(537673.73485328211);
      converter.Processor.ConvertedBladePositions[0].Right.Y.Should().Be(5427401.8836027011);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(1478);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 16525, $"converter.ProcessedCellPassCount != 16525 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1478, $"converter.ProcessedEpochCount != 1478, [= {converter.ProcessedEpochCount}]");
    }

    [Fact()]
    public void Test_No_ACS_Coordinate_Conversion()
    {
      var converter = DITagFileFixture.ReadTAGFile("TestTAGFile-CMV-1.tag", Guid.NewGuid(), false);
      Assert.False(converter.IsUTMCoordinateSystem, "Tagfile should not be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().HaveCount(0);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(0);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
      Assert.True(converter.ProcessedCellPassCount == 2810, $"converter.ProcessedCellPassCount != 2810 [={converter.ProcessedCellPassCount}]");
      Assert.True(converter.ProcessedEpochCount == 1428, $"converter.ProcessedEpochCount != 1428, [= {converter.ProcessedEpochCount}]");
    }

  }
}
