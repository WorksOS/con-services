using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.Designs.GridFabric
{
  public class AlignmentDesignGeometryResponseTests
  {
    [Fact]
    public void Creation()
    {
      var response = new AlignmentDesignGeometryResponse();
      response.Should().NotBeNull();
    }

    [Fact]
    public void Creation2()
    {
      var vertices = new[] {new double[] {1, 2, 3}};
      var labels = new [] {new AlignmentGeometryResponseLabel(1, 2, 3, 4),};

      var response = new AlignmentDesignGeometryResponse(DesignProfilerRequestResult.OK, vertices, labels);
      response.Should().NotBeNull();

      response.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
      response.Vertices.Should().BeEquivalentTo(vertices);
      response.Labels.Should().BeEquivalentTo(labels);
    }
  }
}
