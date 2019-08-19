using System;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.OverrideEvents
{
  public class ToFromBinary_OverrideEventRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_OverrideEventRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<OverrideEventRequestArgument>("Empty OverrideEventRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_OverrideEventRequestArgument()
    {
      var argument = new OverrideEventRequestArgument
      {
        Undo = true,
        ProjectID = Guid.NewGuid(),
        AssetID = Guid.NewGuid(),
        StartUTC = DateTime.UtcNow.AddMinutes(-2),
        EndUTC = DateTime.UtcNow,
        MachineDesignName = "some design",
        LayerID = 2
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom OverrideEventRequestArgument not same after round trip serialisation");
    }
  }
}
