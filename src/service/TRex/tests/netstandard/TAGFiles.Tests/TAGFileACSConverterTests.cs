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
    public void Test_ACS_Coordinate_Conversion_Dimensions()
    {
      var targetSiteModel = BuildModel();
      DITAGFileAndSubGridRequestsWithIgniteFixture.AddCSIBToSiteModel(ref targetSiteModel, TestCommonConsts.DIMENSIONS_2012_DC_CSIB);
      var converter = DITagFileFixture.ReadTAGFile("ACS--Dimensions--CATACOM--121030175139.tag", Guid.NewGuid(), false, ref targetSiteModel);
      Assert.True(converter.IsUTMCoordinateSystem, "Tagfile should be ACS coordinate system");
      converter.Processor.ConvertedBladePositions.Should().NotBeNull();
      converter.Processor.ConvertedBladePositions.Should().HaveCount(429);
      converter.Processor.ConvertedBladePositions[0].Left.X.Should().BeApproximately(2740.5499, 0.01);
      converter.Processor.ConvertedBladePositions[0].Left.Y.Should().BeApproximately(1171.5488, 0.01);
      converter.Processor.ConvertedBladePositions[0].Left.Z.Should().BeApproximately(623.91649, 0.01);
      converter.Processor.ConvertedBladePositions[0].Right.X.Should().BeApproximately(2740.592, 0.01);
      converter.Processor.ConvertedBladePositions[0].Right.Y.Should().BeApproximately(1169.576, 0.01);
      converter.Processor.ConvertedBladePositions[0].Right.Z.Should().BeApproximately(623.7330, 0.01);
      converter.Processor.ConvertedRearAxlePositions.Should().HaveCount(0);
      converter.Processor.ConvertedTrackPositions.Should().HaveCount(0);
      converter.Processor.ConvertedWheelPositions.Should().HaveCount(0);
      converter.ProcessedCellPassCount.Should().Be(1003);
      converter.ProcessedEpochCount.Should().Be(429);
      Assert.True(converter.ReadResult == TAGReadResult.NoError, $"converter.ReadResult == TAGReadResult.NoError [= {converter.ReadResult}");
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
