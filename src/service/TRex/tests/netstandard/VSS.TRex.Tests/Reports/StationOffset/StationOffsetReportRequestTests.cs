using System;
using System.Linq;
using System.Threading.Tasks;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Cells;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGrids.GridFabric.ComputeFuncs;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.StationOffset
{
  [UnitTestCoveredRequest(RequestType = typeof(StationOffsetReportRequest_ApplicationService))]
  [UnitTestCoveredRequest(RequestType = typeof(StationOffsetReportRequest_ClusterCompute))]
  // Note: further compute tests in: StationOffsetClusterComputeTests
  public class StationOffsetReportRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private const float ELEVATION_INCREMENT_1_0 = 1.0f;

    private void AddApplicationGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
    <IComputeFunc<StationOffsetReportRequestArgument_ApplicationService,
        StationOffsetReportRequestResponse_ApplicationService>, 
      StationOffsetReportRequestArgument_ApplicationService, 
      StationOffsetReportRequestResponse_ApplicationService>();

    private void AddClusterComputeGridRouting()
    {
      IgniteMock.Immutable.AddClusterComputeGridRouting
      <IComputeFunc<StationOffsetReportRequestArgument_ClusterCompute,
          StationOffsetReportRequestResponse_ClusterCompute>,
        StationOffsetReportRequestArgument_ClusterCompute,
        StationOffsetReportRequestResponse_ClusterCompute>();

      IgniteMock.Immutable.AddClusterComputeGridRouting<SubGridProgressiveResponseRequestComputeFunc, ISubGridProgressiveResponseRequestComputeFuncArgument, bool>();
    }

    private StationOffsetReportRequestArgument_ApplicationService SimpleStationOffsetReportRequestArgument_ApplicationService(ISiteModel siteModel, bool withOverrides)
    {
      return new StationOffsetReportRequestArgument_ApplicationService
      {
        TRexNodeID = "'Test_StationOffsetReportRequest_Execute_EmptySiteModel",
        ProjectID = siteModel.ID,
        Filters = new FilterSet(new CombinedFilter()),
        AlignmentDesignUid = Guid.NewGuid(),
        CrossSectionInterval = 50,
        StartStation = 300,
        EndStation = 800, 
        Offsets = new double[] {-1},
        ReportElevation = true,
        ReportCmv = true,
        Overrides = withOverrides ? new OverrideParameters { OverrideMachineCCV = true, OverridingMachineCCV = 123 } : null
      };
    }
    
    [Fact]
    public void StationOffsetReport_Creation()
    {
      var request = new StationOffsetReportRequest_ApplicationService();

      request.Should().NotBeNull();
    }

    [Fact]
    public async Task StationOffsetReport_EmptySiteModel()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var request = new StationOffsetReportRequest_ApplicationService();

      var response = await request.ExecuteAsync(SimpleStationOffsetReportRequestArgument_ApplicationService(siteModel, false));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.NoProductionDataFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StationOffsetReport_SiteModelWithSingleCell(bool withOverrides)
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = BuildModelForSingleCellElevationAndCmv(ELEVATION_INCREMENT_1_0);

      var request = new StationOffsetReportRequest_ApplicationService();
      var response = await request.ExecuteAsync(SimpleStationOffsetReportRequestArgument_ApplicationService(siteModel, withOverrides));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.ReportType.Should().Be(ReportType.StationOffset);
      response.StationOffsetReportDataRowList.Should().NotBeNull();
      response.StationOffsetReportDataRowList.Count.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Station.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Offsets.Count.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Offsets[0].Cmv.Should().Be(34);//S&O doesn't use override targets
      response.StationOffsetReportDataRowList[0].Offsets[0].Elevation.Should().Be(10);
    }

    private ISiteModel BuildModelForSingleCellElevationAndCmv(float elevationIncrement)
    {
      var baseTime = DateTime.UtcNow;
      byte baseElevation = 1;

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;

      // vibrationState is needed to get cmv values
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(baseTime, VibrationState.On);
      siteModel.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(baseTime, AutoVibrationState.Manual);

      siteModel.MachinesTargetValues[0].SaveMachineEventsToPersistentStore(siteModel.PrimaryStorageProxy);

      var cellPasses = Enumerable.Range(0, 10).Select(x =>
        new CellPass
        {
          InternalSiteModelMachineIndex = bulldozerMachineIndex,
          Time = baseTime.AddMinutes(x),
          Height = (byte)(baseElevation + x * elevationIncrement),
          PassType = PassType.Front,
          CCV = 34
        }).ToArray();

      DITAGFileAndSubGridRequestsFixture.AddSingleCellWithPasses
        (siteModel, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, cellPasses, 1, cellPasses.Length);

      DITAGFileAndSubGridRequestsFixture.ConvertSiteModelToImmutable(siteModel);

      return siteModel;
    }
  }
}
