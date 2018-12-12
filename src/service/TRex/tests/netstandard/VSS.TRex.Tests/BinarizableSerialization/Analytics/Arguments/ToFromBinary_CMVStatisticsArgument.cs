using System;
using VSS.TRex.Analytics.CMVStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_CMVStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CMVStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CMVStatisticsArgument>("Empty CMVStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CMVStatisticsArgument()
    {
      var argument = new CMVStatisticsArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesignUID = Guid.NewGuid(),
        CMVPercentageRange = new CMVRangePercentageRecord(80, 120),
        OverrideMachineCMV = false,
        OverridingMachineCMV = 50,
        CMVDetailValues = new[] { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CMVStatisticsArgument not same after round trip serialisation");
    }
  }
}
