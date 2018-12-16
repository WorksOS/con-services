using System;
using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_CutFillStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_CutFillStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<CutFillStatisticsArgument>("Empty CutFillStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_CutFillStatisticsArgument()
    {
      var argument = new CutFillStatisticsArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesignUID = Guid.NewGuid(),
        DesignID = Guid.NewGuid(),
        Offsets = new[] { 0.5, 0.2, 0.1, 0, -0.1, -0.2, -0.5 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom CutFillStatisticsArgument not same after round trip serialisation");
    }
  }
}
