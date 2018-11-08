using System;
using VSS.TRex.Analytics.TemperatureStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_TemperatureStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_TemperatureStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<TemperatureStatisticsArgument>("Empty TemperatureStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_TemperatureStatisticsArgument()
    {
      var argument = new TemperatureStatisticsArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesignID = Guid.NewGuid(),
        OverrideTemperatureWarningLevels = false,
        OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord(100, 200),
        TemperatureDetailValues = new[] { 0, 120, 140, 160, 4000 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom TemperatureStatisticsArgument not same after round trip serialisation");
    }
  }
}
