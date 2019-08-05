using System;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_MDPStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_MDPStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<MDPStatisticsArgument>("Empty MDPStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_MDPStatisticsArgument()
    {
      var argument = new MDPStatisticsArgument()
      {
        TRexNodeID = "1",
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        Overrides = new OverrideParameters
        { 
          MDPRange = new MDPRangePercentageRecord(80, 120),
          OverrideMachineMDP = false,
          OverridingMachineMDP = 1000
        }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom MDPStatisticsArgument not same after round trip serialisation");
    }
  }
}
