using System.Threading.Tasks;
using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.ComputeFuncs;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  [UnitTestCoveredRequest(RequestType = typeof(AlignmentDesignGeometryRequest))]
  public class AlignmentDesignGeometryRequestTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddDesignProfilerGridRouting() => IgniteMock.Immutable.AddApplicationGridRouting
      <AlignmentDesignGeometryComputeFunc, AlignmentDesignGeometryArgument, AlignmentDesignGeometryResponse>();

    [Fact]
    public void Creation()
    {
      var request = new AlignmentDesignFilterBoundaryRequest();

      request.Should().NotBeNull();
    }

    [Fact]
    public async Task Geometry()
    {
      AddDesignProfilerGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var designUid = DITAGFileAndSubGridRequestsWithIgniteFixture.AddSVLAlignmentDesignToSiteModel(ref siteModel, TestHelper.CommonTestDataPath, "Large Sites Road - Trimble Road.svl", false);

      var request = new AlignmentDesignGeometryRequest();
      var response = await request.ExecuteAsync(new AlignmentDesignGeometryArgument
      {
        ProjectID = siteModel.ID,
        AlignmentDesignID = designUid
      });

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().NotBeNull();
      response.Vertices.Length.Should().Be(2);

      response.Arcs.Should().NotBeNull();
      response.Arcs.Length.Should().Be(2);

      response.Labels.Should().NotBeNull();
      response.Labels.Length.Should().Be(21);
    }
  }
}
