using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AlignmentDesignStationRangeRequest))]
  public class AlignmentDesignStationRangeRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <AlignmentDesignStationRangeComputeFunc, DesignSubGridRequestArgumentBase, AlignmentDesignStationRangeResponse>();

    [Fact]
    public void Test_AlignmentDesignStationRangeRequest_Creation()
    {
      var request = new AlignmentDesignStationRangeRequest();

      request.Should().NotBeNull();
    }

    [Fact(Skip="BUG#85914")]
    public async Task Test_AlignmentDesignStationRangeRequest()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm", false);
      var referenceDesign = new DesignOffset(designUid, 0);

      var request = new AlignmentDesignStationRangeRequest();
      var response = await request.ExecuteAsync(new DesignSubGridRequestArgumentBase
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        Filters = new FilterSet(new CombinedFilter()),
        TRexNodeID = "UnitTest_TRexNodeID"
      });

      // TODO To complete this test later once an alignment design implementation becomes available on a .Net standard version of the Symphony SDK
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
    }
  }
}
