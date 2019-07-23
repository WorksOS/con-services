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
  [UnitTestCoveredRequest(RequestType = typeof(AlignmentDesignFilterBoundaryRequest))]
  public class AlignmentDesignFilterBoundaryRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.AddApplicationGridRouting
      <AlignmentDesignFilterBoundaryComputeFunc, AlignmentDesignFilterBoundaryArgument, AlignmentDesignFilterBoundaryResponse>();

    [Fact]
    public void Test_AlignmentDesignFilterBoundaryRequest_Creation()
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      request.Should().NotBeNull();
    }

    [Fact(Skip="See BUG#85914")]
    public async Task Test_AlignmentDesignFilterBoundaryRequest()
    {
      const double START_STATION = 0.0;
      const double END_STATION = 100.0;
      const double LEFT_OFFSET = 1.0;
      const double RIGHT_OFFSET = 2.0;

      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Bug36372.ttm", false);
      var referenceDesign = new DesignOffset(designUid, 0);

      var request = new AlignmentDesignFilterBoundaryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignFilterBoundaryArgument()
      {
        ProjectID = siteModel.ID,
        ReferenceDesign = referenceDesign,
        Filters = new FilterSet(new CombinedFilter()),
        TRexNodeID = "UnitTest_TRexNodeID",
        StartStation = START_STATION,
        EndStation = END_STATION,
        LeftOffset = LEFT_OFFSET,
        RightOffset = RIGHT_OFFSET
      });

      // TODO To complete this test later once an alignment filter boundary implementation becomes available on a .Net standard version of the Symphony SDK
      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
    }
  }
}
