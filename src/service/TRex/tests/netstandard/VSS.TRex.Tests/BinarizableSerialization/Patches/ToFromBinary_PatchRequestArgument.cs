using System;
using VSS.TRex.Exports.Patches.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Patches
{
  public class ToFromBinary_PatchRequestArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_PatchRequestArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PatchRequestArgument>("Empty PatchRequestArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_PatchRequestArgument()
    {
      var argument = new PatchRequestArgument()
      {
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        DataPatchNumber = 0,
        DataPatchSize = 10,
        Mode = DisplayMode.Height
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom PatchRequestArgument not same after round trip serialisation");
    }
  }
}
