using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_AlignmentDesignFilterBoundaryRequest : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Test_AlignmentDesignFilterBoundaryRequest_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<AlignmentDesignFilterBoundaryRequest>("Empty AlignmentDesignFilterBoundaryRequest not same after round trip serialisation");
    }

    [Fact]
    public void Test_AlignmentDesignFilterBoundaryResponse_Polygon()
    {
      var response = new AlignmentDesignFilterBoundaryRequest
      {
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom AlignmentDesignFilterBoundaryRequest not same after round trip serialisation");
    }
  }
}
