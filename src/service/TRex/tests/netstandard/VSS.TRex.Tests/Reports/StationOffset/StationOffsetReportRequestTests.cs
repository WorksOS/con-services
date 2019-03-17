using System;
using System.Linq;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.Cells;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
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
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      return siteModel;
    }
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting
    <IComputeFunc<StationOffsetReportRequestArgument_ApplicationService,
        StationOffsetReportRequestResponse_ApplicationService>, 
      StationOffsetReportRequestArgument_ApplicationService, 
      StationOffsetReportRequestResponse_ApplicationService>();

    private void AddClusterComputeGridRouting() => IgniteMock.AddClusterComputeGridRouting
    <IComputeFunc<StationOffsetReportRequestArgument_ClusterCompute,
        StationOffsetReportRequestResponse_ClusterCompute>,
      StationOffsetReportRequestArgument_ClusterCompute,
      StationOffsetReportRequestResponse_ClusterCompute>();

    private StationOffsetReportRequestArgument_ApplicationService SimpleStationOffsetReportRequestArgument_ApplicationService(ISiteModel siteModel)
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
        ReportCmv = true
      };
    }
    
    [Fact]
    public void StationOffsetReport_Creation()
    {
      var request = new StationOffsetReportRequest_ApplicationService();

      request.Should().NotBeNull();
    }

    [Fact]
    public void StationOffsetReport_EmptySiteModel()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = NewEmptyModel();
      var request = new StationOffsetReportRequest_ApplicationService();

      var response = request.Execute(SimpleStationOffsetReportRequestArgument_ApplicationService(siteModel));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.NoProductionDataFound);
    }

    [Fact]
    public void StationOffsetReport_SiteModelWithSingleCell()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      BuildModelForSingleCellElevationAndCmv(out var siteModel, 1);

      var request = new StationOffsetReportRequest_ApplicationService();
      var response = request.Execute(SimpleStationOffsetReportRequestArgument_ApplicationService(siteModel));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.OK);
      response.ReportType.Should().Be(ReportType.StationOffset);
      response.StationOffsetReportDataRowList.Should().NotBeNull();
      response.StationOffsetReportDataRowList.Count.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Station.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Offsets.Count.Should().Be(1);
      response.StationOffsetReportDataRowList[0].Offsets[0].Cmv.Should().Be(34);
      response.StationOffsetReportDataRowList[0].Offsets[0].Elevation.Should().Be(10);
    }

    private void BuildModelForSingleCellElevationAndCmv(out ISiteModel siteModel, float elevationIncrement)
    {
      var baseTime = DateTime.UtcNow;
      byte baseElevation = 1;

      siteModel = NewEmptyModel();
      var bulldozerMachineIndex = siteModel.Machines.Locate("Bulldozer", false).InternalSiteModelMachineIndex;
      // vibrationState is needed to get cmv values
      siteModel.MachinesTargetValues[0].VibrationStateEvents.PutValueAtDate(baseTime, VibrationState.On);
      siteModel.MachinesTargetValues[0].AutoVibrationStateEvents.PutValueAtDate(baseTime, AutoVibrationState.Manual);

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
    }

  }
}
