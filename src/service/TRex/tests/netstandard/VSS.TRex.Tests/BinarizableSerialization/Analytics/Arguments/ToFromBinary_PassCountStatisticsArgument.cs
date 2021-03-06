﻿using System;
using VSS.TRex.Analytics.PassCountStatistics.GridFabric;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Records;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization.Analytics.Arguments
{
  public class ToFromBinary_PassCountStatisticsArgument : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_PassCountStatisticsArgument_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<PassCountStatisticsArgument>("Empty PassCountStatisticsArgument not same after round trip serialisation");
    }

    [Fact]
    public void Test_PassCountStatisticsArgument()
    {
      var argument = new PassCountStatisticsArgument()
      {
        TRexNodeID = Guid.NewGuid(),
        ProjectID = Guid.NewGuid(),
        Filters = new FilterSet(new CombinedFilter()),
        ReferenceDesign = new DesignOffset(Guid.NewGuid(), 1.5),
        Overrides = new OverrideParameters
        { 
          OverridingTargetPassCountRange = new PassCountRangeRecord(3, 10),
          OverrideTargetPassCount = false,
        },
        PassCountDetailValues = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
      };

      SimpleBinarizableInstanceTester.TestClass(argument, "Custom PassCountStatisticsArgument not same after round trip serialisation");
    }
  }
}
