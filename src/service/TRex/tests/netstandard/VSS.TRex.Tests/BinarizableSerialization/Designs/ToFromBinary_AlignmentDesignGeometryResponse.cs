using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_AlignmentDesignGeometryResponse
  {
    [Fact]
    public void Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<AlignmentDesignGeometryResponse>("Empty AlignmentDesignGeometryResponse not same after round trip serialisation");
    }

    [Fact]
    public void WithData()
    {
      var response = new AlignmentDesignGeometryResponse
      {
        RequestResult = DesignProfilerRequestResult.OK,
        Vertices = new[] { new double[] {0, 1, 0}, new double[] { 2, 3, 1} },
        Labels = new[] {
          new AlignmentGeometryResponseLabel(0.0, 1.0, 1.1, 0.5),
          new AlignmentGeometryResponseLabel(100.0, 1.01, 1.11, 0.75)
        }
      };

      var result = SimpleBinarizableInstanceTester.TestClass(response, "Custom AlignmentDesignGeometryResponse not same after round trip serialisation");

      result.member.Should().BeEquivalentTo(response);
    }
  }
}
