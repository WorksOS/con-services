using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.OverrideEvents
{
  public class ToFromBinary_OverrideEventResponse : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_OverrideEventResponse_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<OverrideEventResponse>("Empty OverrideEventResponse not same after round trip serialisation");
    }

    [Fact]
    public void Test_OverrideEventResponse_WithContent()
    {
      var response = new OverrideEventResponse
      {
        Message = "This is an error message",
        Success = false
      };

      SimpleBinarizableInstanceTester.TestClass(response, "Custom OverrideEventResponse not same after round trip serialisation");
    }
  }
}

