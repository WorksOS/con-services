using System;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_SpeedStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_SpeedStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<SpeedStatisticsArgument>("Empty SpeedStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_SpeedStatisticsArgument()
    {
      var argument = new SpeedStatisticsArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesignID = Guid.NewGuid(),
        TargetMachineSpeed = new MachineSpeedExtendedRecord(5, 50)
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom SpeedStatisticsArgument not same after round trip serialisation");
    }
  }
}
