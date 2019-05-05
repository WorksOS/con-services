using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.Caching;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.ComputeFuncs;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(SurfaceElevationPatchRequest))]
  public class SurfaceElevationPatchRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<SurfaceElevationPatchComputeFunc, ISurfaceElevationPatchArgument, byte[]>();

    [Fact]
    public void Creation()
    {
      var req = new SurfaceElevationPatchRequest();
      req.Should().NotBeNull();
    }

    [Fact]
    public void Creation_WithCache()
    {
      var cache = new TRexSpatialMemoryCache(100, 1000, 0.5);
      var context = cache.LocateOrCreateContext(Guid.NewGuid(), "Creation_WithCache", TimeSpan.FromHours(1));

      var req = new SurfaceElevationPatchRequest(cache, context);
      req.Should().NotBeNull();
    }

    private void ValidateSurveyedSurfaceSuGridResult(ISiteModel siteModel, GridDataType expectedGridType, 
      SurveyedSurfacePatchType patchType, IClientLeafSubGrid result)
    {
      result.Should().NotBeNull();
      result.CellSize.Should().Be(siteModel.CellSize);
      result.IndexOriginOffset.Should().Be(SubGridTreeConsts.DefaultIndexOriginOffset);
      result.Level.Should().Be(SubGridTreeConsts.SubGridTreeLevels);
      result.WorldExtents().Should().BeEquivalentTo(new BoundingWorldExtent3D(0, 0, SubGridTreeConsts.SubGridTreeDimension * siteModel.CellSize, SubGridTreeConsts.SubGridTreeDimension * siteModel.CellSize));

      result.GridDataType.Should().Be(expectedGridType);
      if (patchType == SurveyedSurfacePatchType.EarliestSingleElevation || patchType == SurveyedSurfacePatchType.LatestSingleElevation)
        result.Should().BeOfType<ClientHeightAndTimeLeafSubGrid>();
      else
        result.Should().BeOfType<ClientCompositeHeightsLeafSubgrid>();

      result.CountNonNullCells().Should().Be(903);
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations, GridDataType.CompositeHeights)]
    public void Execute_SingleSurveyedSurface(SurveyedSurfacePatchType patchType, GridDataType expectedGridType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);

      var req = new SurfaceElevationPatchRequest();      
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result);
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations, GridDataType.CompositeHeights)]
    public void Execute_SingleSurveyedSurface_WithSpatialCaching(SurveyedSurfacePatchType patchType, GridDataType expectedGridType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);

      var cache = new TRexSpatialMemoryCache(100, 10000, 0.5);
      var context = cache.LocateOrCreateContext(siteModel.ID, $"Execute_SingleSurveyedSurface_WithSpatialCaching-{patchType}", TimeSpan.FromHours(1));
      var req = new SurfaceElevationPatchRequest(cache, context);

      // Make one call which will populate the cache
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result);

      // Make a second call which will utilise the value in the cache
      var result2 = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result2);

      result.Should().BeEquivalentTo(result2);
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations, GridDataType.CompositeHeights)]
    public void Execute_DualIdenticalSurveyedSurfaces(SurveyedSurfacePatchType patchType, GridDataType expectedGridType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);

      var req = new SurfaceElevationPatchRequest();
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result);
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations, GridDataType.CompositeHeights)]
    public void Execute_DualNonOverlappingSurveyedSurfaces(SurveyedSurfacePatchType patchType, GridDataType expectedGridType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceOffsetFromOrigin(ref siteModel, 1.0f, asAtDate, 100, 100);

      var req = new SurfaceElevationPatchRequest();
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result);
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation, GridDataType.HeightAndTime)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations, GridDataType.CompositeHeights)]
    public void Execute_DualSurveyedSurfaces_DifferingInTime(SurveyedSurfacePatchType patchType, GridDataType expectedGridType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate.AddMinutes(1));

      var filteredSurveyedSurfaces = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      siteModel.SurveyedSurfaces.FilterSurveyedSurfaceDetails(false, Consts.MIN_DATETIME_AS_UTC, Consts.MIN_DATETIME_AS_UTC, 
        false, filteredSurveyedSurfaces, new Guid[0]);

      var req = new SurfaceElevationPatchRequest();
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, filteredSurveyedSurfaces));

      ValidateSurveyedSurfaceSuGridResult(siteModel, expectedGridType, patchType, result);

      if (expectedGridType == GridDataType.HeightAndTime)
      {
        // Check the times on the result cells are correct
        result.ForEach((x, y) =>
        {
          if (((ClientHeightAndTimeLeafSubGrid) result).Cells[x, y] != Consts.NullHeight)
            ((ClientHeightAndTimeLeafSubGrid) result).Times[x, y].Should().Be
              (patchType == SurveyedSurfacePatchType.EarliestSingleElevation ? asAtDate.Ticks : asAtDate.AddMinutes(1).Ticks);
        });
      }
    }

    [Theory]
    [InlineData(SurveyedSurfacePatchType.EarliestSingleElevation)]
    [InlineData(SurveyedSurfacePatchType.LatestSingleElevation)]
    [InlineData(SurveyedSurfacePatchType.CompositeElevations)]
    public void Execute_FailWithNonExistentDesignReference(SurveyedSurfacePatchType patchType)
    {
      AddApplicationGridRouting();

      var asAtDate = DateTime.UtcNow;
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      // Construct a surveyed surface from the design
      DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleSurveyedSurfaceAboutOrigin(ref siteModel, 1.0f, asAtDate);

      // Delete the second surveyed surface TTM file from both it's original location and the project temporary folder
      string fileToDelete = siteModel.SurveyedSurfaces[0].DesignDescriptor.FullPath;
      File.Exists(fileToDelete).Should().BeTrue();
      File.Delete(fileToDelete);
      File.Exists(fileToDelete).Should().BeFalse();

      fileToDelete = Path.Combine(FilePathHelper.GetTempFolderForProject(siteModel.ID), siteModel.SurveyedSurfaces[0].DesignDescriptor.FileName);
      File.Exists(fileToDelete).Should().BeTrue();
      File.Delete(fileToDelete);
      File.Exists(fileToDelete).Should().BeFalse();

      var req = new SurfaceElevationPatchRequest();
      var result = req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, patchType, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces));

      result.Should().BeNull();
    }

    [Fact]
    public void Execute_WithInvalidPatchType()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var req = new SurfaceElevationPatchRequest();

      req.Execute(new SurfaceElevationPatchArgument(siteModel.ID, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        siteModel.CellSize, (SurveyedSurfacePatchType) 100, SubGridTreeBitmapSubGridBits.FullMask, siteModel.SurveyedSurfaces)).Should().BeNull();
    }
  }
}
