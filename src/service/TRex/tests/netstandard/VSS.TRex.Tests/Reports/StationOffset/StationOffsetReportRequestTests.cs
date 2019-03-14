using System;
using Apache.Ignite.Core.Compute;
using FluentAssertions;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
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
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceType.SNM940, false, Guid.NewGuid());
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
        Offsets = new double[] {-1,0,1},
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
    public void StationOffsetReport_EmptySiteModel_UsingTemporaryDummyStations()
    {
      AddClusterComputeGridRouting();
      AddApplicationGridRouting();

      var siteModel = NewEmptyModel();
      var request = new StationOffsetReportRequest_ApplicationService();

      var response = request.Execute(SimpleStationOffsetReportRequestArgument_ApplicationService(siteModel));

      response.Should().NotBeNull();
      response.ResultStatus.Should().Be(RequestErrorStatus.NoProductionDataFound);
    }
  }
}
