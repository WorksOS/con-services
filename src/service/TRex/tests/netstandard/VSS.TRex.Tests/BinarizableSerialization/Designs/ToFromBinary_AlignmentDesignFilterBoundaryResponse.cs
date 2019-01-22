using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Geometry;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_AlignmentDesignFilterBoundaryResponse
  {
    [Fact]
    public void Test_AlignmentDesignFilterBoundaryResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<AlignmentDesignFilterBoundaryResponse>("Empty AlignmentDesignFilterBoundaryResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_AlignmentDesignFilterBoundaryResponse_Polygon()
    {
      var response = new AlignmentDesignFilterBoundaryResponse
      {
         Boundary = new Fence(1, 2, 3, 4)
         {
           IsRectangle = false
         }
      };

      var result = SimpleBinarizableInstanceTester.TestClass(response, "Custom AlignmentDesignFilterBoundaryResponse not same after round trip serialisation");

      result.member.Boundary.HasVertices.Should().Be(true);
      result.member.Boundary.NumVertices.Should().Be(4);
      result.member.Boundary.IsRectangle.Should().Be(false);
    }

    [Fact]
    public void Test_AlignmentDesignFilterBoundaryResponse_Rectangle()
    {
      var response = new AlignmentDesignFilterBoundaryResponse
      {
        Boundary = new Fence(0, 0, 100, 100)
        {
          IsRectangle = true
        }
      };

      var result = SimpleBinarizableInstanceTester.TestClass(response, "Custom AlignmentDesignFilterBoundaryResponse not same after round trip serialisation");

      result.member.Boundary.IsRectangle.Should().Be(true);
      result.member.Boundary.HasVertices.Should().Be(true);
      result.member.Boundary.NumVertices.Should().Be(4);
      result.member.Boundary.Area().Should().Be(10000);
    }
  }
}
