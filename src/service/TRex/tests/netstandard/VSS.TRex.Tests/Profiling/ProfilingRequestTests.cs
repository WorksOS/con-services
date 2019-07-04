using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
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

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      //This is required to get CCV
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, VibrationState.On);

      //Set machine targets
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetCCVStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 123);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMDPStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 321);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetPassCountStateEvents.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 4);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMinMaterialTemperature.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 652);
      siteModel.MachinesTargetValues[bulldozerMachineIndex].TargetMaxMaterialTemperature.PutValueAtDate(Consts.MIN_DATETIME_AS_UTC, 655);

      //Set up cell passes
      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = x,
          CCV = (short)(123 + x),
          MachineSpeed = (ushort)(456 + x),
          MDP = (short)(321 + x),
          MaterialTemperature = (ushort)(652 + x),          
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
        ProfileTypeRequired = GridDataType.CCV,//Note: GridDataType.Height doesn't do the population for CCV
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
      response.ProfileCells[0].CellCCV.Should().Be(132);//123+9
      response.ProfileCells[0].CellCCVElev.Should().Be(9);
      response.ProfileCells[0].CellTargetCCV.Should().Be(123);
      response.ProfileCells[0].CellPreviousMeasuredCCV.Should().Be(131);
      response.ProfileCells[0].CellPreviousMeasuredTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[0].CellMDP.Should().Be(330);//321+9
      response.ProfileCells[0].CellMDPElev.Should().Be(9);
      response.ProfileCells[0].CellTargetMDP.Should().Be(321);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMin.Should().Be(4);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMax.Should().Be(4);
      response.ProfileCells[0].TopLayerPassCount.Should().Be(10);
      response.ProfileCells[0].CellMinSpeed.Should().Be(456);
      response.ProfileCells[0].CellMaxSpeed.Should().Be(465);//456+9
      response.ProfileCells[0].CellMaterialTemperatureWarnMin.Should().Be(652);
      response.ProfileCells[0].CellMaterialTemperatureWarnMax.Should().Be(655);
      response.ProfileCells[0].CellMaterialTemperature.Should().Be(661);//652+9
      response.ProfileCells[0].CellMaterialTemperatureElev.Should().Be(9);

      response.ProfileCells[1].CellFirstElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLastElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLowestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellHighestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellCCVElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].CellMDPElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMin.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMax.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCount.Should().Be(CellPassConsts.NullPassCountValue);
      //Note: MinSpeed of Null and MaxSpeed of 0 are the defaults meaning no speed values
      response.ProfileCells[1].CellMinSpeed.Should().Be(CellPassConsts.NullMachineSpeed);
      response.ProfileCells[1].CellMaxSpeed.Should().Be(0);
      response.ProfileCells[1].CellMaterialTemperatureWarnMin.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureWarnMax.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureElev.Should().Be(CellPassConsts.NullHeight);
    }

    [Fact]
    public void ProfileCell_SingleCell_WithOverrides()
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
        ReturnAllPassesAndLayers = false,
        Overrides = new OverrideParameters
        {
          OverrideMachineCCV = true,
          OverridingMachineCCV = 987,
          OverrideMachineMDP = true,
          OverridingMachineMDP = 789,
          OverrideTargetPassCount = true,
          OverridingTargetPassCountRange = new PassCountRangeRecord(5, 6),
          OverrideTemperatureWarningLevels = true,
          OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(400, 1200),
          TargetMachineSpeed = new MachineSpeedExtendedRecord(777, 888)
        }
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
      response.ProfileCells[0].CellCCV.Should().Be(132);//123+9
      response.ProfileCells[0].CellCCVElev.Should().Be(9);
      response.ProfileCells[0].CellTargetCCV.Should().Be(987);
      response.ProfileCells[0].CellPreviousMeasuredCCV.Should().Be(131);
      response.ProfileCells[0].CellPreviousMeasuredTargetCCV.Should().Be(987);
      response.ProfileCells[0].CellMDP.Should().Be(330);//321+9
      response.ProfileCells[0].CellMDPElev.Should().Be(9);
      response.ProfileCells[0].CellTargetMDP.Should().Be(789);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMin.Should().Be(5);
      response.ProfileCells[0].TopLayerPassCountTargetRangeMax.Should().Be(6);
      response.ProfileCells[0].TopLayerPassCount.Should().Be(10);
      response.ProfileCells[0].CellMinSpeed.Should().Be(456);
      response.ProfileCells[0].CellMaxSpeed.Should().Be(465);//456+9
      response.ProfileCells[0].CellMaterialTemperatureWarnMin.Should().Be(400);
      response.ProfileCells[0].CellMaterialTemperatureWarnMax.Should().Be(1200);
      response.ProfileCells[0].CellMaterialTemperature.Should().Be(661);//652+9
      response.ProfileCells[0].CellMaterialTemperatureElev.Should().Be(9);

      response.ProfileCells[1].CellFirstElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLastElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellLowestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellHighestElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellCCVElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellPreviousMeasuredTargetCCV.Should().Be(CellPassConsts.NullCCV);
      response.ProfileCells[1].CellMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].CellMDPElev.Should().Be(CellPassConsts.NullHeight);
      response.ProfileCells[1].CellTargetMDP.Should().Be(CellPassConsts.NullMDP);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMin.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCountTargetRangeMax.Should().Be(CellPassConsts.NullPassCountValue);
      response.ProfileCells[1].TopLayerPassCount.Should().Be(CellPassConsts.NullPassCountValue);
      //Note: MinSpeed of Null and MaxSpeed of 0 are the defaults meaning no speed values
      response.ProfileCells[1].CellMinSpeed.Should().Be(CellPassConsts.NullMachineSpeed);
      response.ProfileCells[1].CellMaxSpeed.Should().Be(0);
      response.ProfileCells[1].CellMaterialTemperatureWarnMin.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureWarnMax.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperature.Should().Be(CellPassConsts.NullMaterialTemperatureValue);
      response.ProfileCells[1].CellMaterialTemperatureElev.Should().Be(CellPassConsts.NullHeight);
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
