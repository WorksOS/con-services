using System;
using FluentAssertions;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Reports.Gridded.GridFabric;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Reports.Gridded
{
  [UnitTestCoveredRequest(RequestType = typeof(GriddedReportRequest))]
  public class GriddedReportRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Test_GriddedReportRequest_Creation()
    {
      var request = new GriddedReportRequest();
      request.Should().NotBeNull();
    }
    private void AddApplicationGridRouting() => IgniteMock.AddApplicationGridRouting<GriddedReportRequestComputeFunc, GriddedReportRequestArgument, GriddedReportRequestResponse>();
    
    private ISiteModel NewEmptyModel()
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(DITagFileFixture.NewSiteModelGuid, true);
      _ = siteModel.Machines.CreateNew("Bulldozer", "", MachineType.Dozer, DeviceTypeEnum.SNM940, false, Guid.NewGuid());
      return siteModel;
    }

    private GriddedReportRequestArgument SimpleGriddedReportRequestArgument(Guid projectUid)
    {
      return new GriddedReportRequestArgument
      { 
        GridInterval = 2,
        Filters = new FilterSet(new CombinedFilter()),
        GridReportOption = GridReportOption.Automatic,
        StartNorthing = 800,
        StartEasting = 300,
        EndNorthing = 1000,
        EndEasting = 500,
        Azimuth = 0,
        TRexNodeID = "'Test_GriddedReportRequest_Execute_EmptySiteModel",
        ProjectID = projectUid
      };
    }

    [Fact]
    public void Test_GriddedReportRequest_Execute_EmptySiteModel()
    {
      AddApplicationGridRouting();
      var siteModel = NewEmptyModel();
      var request = new GriddedReportRequest();
      var response = request.Execute(SimpleGriddedReportRequestArgument(siteModel.ID));

      response.Should().NotBeNull();
    }
  }
}


