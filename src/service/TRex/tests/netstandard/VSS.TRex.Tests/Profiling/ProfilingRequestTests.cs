using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.ComputeFuncs;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.GridFabric.Responses;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Profiling
{
  [UnitTestCoveredRequest(RequestType = typeof(ProfileRequest_ApplicationService_ProfileCell))]
  [UnitTestCoveredRequest(RequestType = typeof(ProfileRequest_ApplicationService_SummaryVolumeProfileCell))]
  public class ProfilingRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {

    private void AddDesignProfilerGridRouting()
    {
      //This is specific to cell datum i.e. what the cell datum cluster compute will call in the design profiler
      IgniteMock.AddApplicationGridRouting<CalculateDesignElevationPatchComputeFunc, CalculateDesignElevationPatchArgument, CalculateDesignElevationPatchResponse>();
    }

    private void AddApplicationGridRouting()
    {
      IgniteMock.AddApplicationGridRouting<ProfileRequestComputeFunc_ApplicationService<ProfileCell>, ProfileRequestArgument_ApplicationService, ProfileRequestResponse<ProfileCell>>();
      IgniteMock.AddApplicationGridRouting<ProfileRequestComputeFunc_ApplicationService<SummaryVolumeProfileCell>, ProfileRequestArgument_ApplicationService, ProfileRequestResponse<SummaryVolumeProfileCell>>();
    }

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.AddClusterComputeGridRouting<ProfileRequestComputeFunc_ClusterCompute<ProfileCell>, ProfileRequestArgument_ClusterCompute, ProfileRequestResponse<ProfileCell>>();
      IgniteMock.AddClusterComputeGridRouting<ProfileRequestComputeFunc_ClusterCompute<SummaryVolumeProfileCell>, ProfileRequestArgument_ClusterCompute, ProfileRequestResponse<SummaryVolumeProfileCell>>();
    }

    private void AddRoutings()
    {
      AddDesignProfilerGridRouting();
      AddApplicationGridRouting();
      AddClusterComputeGridRouting();
    }

    [Fact]
    public void Creation_ProfileCell()
    {
      var req = new ProfileRequest_ApplicationService_ProfileCell();

      req.Should().NotBeNull();
    }

    [Fact]
    public void Creation_SummaryVolumeProfileCell()
    {
      var req = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();

      req.Should().NotBeNull();
    }

    private ISiteModel BuildModelForSingleCell()
    {
      var baseTime = DateTime.UtcNow;
      short baseCMV = 10;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = x,
          PassType = PassType.Front
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);
      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }

    [Fact]
    public void ProfileCell_SingleCell_NoDesign()
    {
      AddRoutings();

      var sm = BuildModelForSingleCell();

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = sm.ID,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = null,
        StartPoint = new WGS84Point(-1.0, sm.Grid.CellSize / 2),
        EndPoint = new WGS84Point(1.0, sm.Grid.CellSize / 2),
        ReturnAllPassesAndLayers = false
      };

      var svRequest = new ProfileRequest_ApplicationService_ProfileCell();
      var response = svRequest.Execute(arg);

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.GridDistanceBetweenProfilePoints.Should().Be(2.0);

      response.ProfileCells.Count.Should().Be(2);

      response.ProfileCells[0].CellFirstElev.Should().Be(0);
      response.ProfileCells[0].CellLastElev.Should().Be(9);
      response.ProfileCells[0].CellLowestElev.Should().Be(0);
      response.ProfileCells[0].CellHighestElev.Should().Be(9);
      response.ProfileCells[1].CellFirstElev.Should().Be(Consts.NullHeight);
      response.ProfileCells[1].CellLowestElev.Should().Be(Consts.NullHeight);
    }

    [Theory]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign, 0.0f, 0.0f, Consts.NullHeight)]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter, 0.0f, Consts.NullHeight, 9.0f)]
    [InlineData(VolumeComputationType.BetweenFilterAndDesign, 10.0f, 0.0f, Consts.NullHeight)]
    [InlineData(VolumeComputationType.BetweenDesignAndFilter, 10.0f, Consts.NullHeight, 9.0f)]
    public void SummaryVolumeProfileCell_SingleCell_FlatDesignAtOrigin_FilterToDesignOrDesignToFilter(VolumeComputationType volumeComputationType, float designElevation,
      float lastPassElevation1, float lastPassElevation2)
    {
      AddRoutings();

      var sm = BuildModelForSingleCell();
      var design = DITAGFileAndSubGridRequestsWithIgniteFixture.ConstructSingleFlatTriangleDesignAboutOrigin(ref sm, designElevation);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = sm.ID,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.SummaryVolume,
        PositionsAreGrid = true,
        Filters = new FilterSet(
          new CombinedFilter
          {
            AttributeFilter = new CellPassAttributeFilter {ReturnEarliestFilteredCellPass = true}
          },
          new CombinedFilter()),
        ReferenceDesign = new DesignOffset(design, 0),
        StartPoint = new WGS84Point(-1.0, sm.Grid.CellSize / 2),
        EndPoint = new WGS84Point(1.0, sm.Grid.CellSize / 2),
        ReturnAllPassesAndLayers = false,
        VolumeType = volumeComputationType
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var svRequest = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();
      var response = svRequest.Execute(arg);

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.ProfileCells.Count.Should().Be(3);
      response.ProfileCells[0].DesignElev.Should().Be(designElevation);
      response.ProfileCells[0].LastCellPassElevation1.Should().Be(lastPassElevation1);
      response.ProfileCells[0].LastCellPassElevation2.Should().Be(lastPassElevation2);
      response.ProfileCells[0].InterceptLength.Should().BeApproximately(sm.Grid.CellSize, 0.001);
      response.ProfileCells[0].OTGCellX.Should().Be(SubGridTreeConsts.DefaultIndexOriginOffset);
      response.ProfileCells[0].OTGCellY.Should().Be(SubGridTreeConsts.DefaultIndexOriginOffset);
    }
  }
}
