using FluentAssertions;
using VSS.TRex.Designs.GridFabric.Responses;
using VSS.TRex.Designs.Models;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Designs
{
  public class ToFromBinary_BaseDesignRequestResponse
  {
    [Fact]
    public void Test_BaseDesignRequestResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<BaseDesignRequestResponse>("Empty BaseDesignRequestResponse not same after round trip serialisation");
    }  

    [Fact]
    public void Test_BaseDesignRequestResponse_Custom()
    {
      var response = new BaseDesignRequestResponse
      {
        RequestResult = DesignProfilerRequestResult.OK
      };

      var result = SimpleBinarizableInstanceTester.TestClass(response, "Custom BaseDesignRequestResponse not same after round trip serialisation");

      result.member.RequestResult.Should().Be(DesignProfilerRequestResult.OK);
    }
  }
}
