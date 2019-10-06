using VSS.TRex.Common.Models;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using VSS.TRex.Types;
using VSS.TRex.Types.Types;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_LiftParameters : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_LiftParameters_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<LiftParameters>("Empty lift parameters not same after round trip serialisation");
    }

    [Fact]
    public void Test_LiftParameters_Defaults()
    {
      var liftParameters = new LiftParameters();
      SimpleBinarizableInstanceTester.TestClass(liftParameters, "Default lift parameters not same after round trip serialisation");
    }

    [Fact]
    public void Test_LiftParameters_Custom()
    {
      var liftParameters = new LiftParameters()
      {
        OverrideMachineThickness = true,
        LiftDetectionType = LiftDetectionType.Tagfile,
        LiftThicknessType = LiftThicknessType.Uncompacted,
        TargetLiftThickness = 0.2,
        MDPSummarizeTopLayerOnly = false,
        MDPSummaryTypes = MDPSummaryTypes.Thickness,
        CCVSummarizeTopLayerOnly = false,
        CCVSummaryTypes = CCVSummaryTypes.WorkInProgress,
        IncludeSuperseded = true,
        FirstPassThickness = 1.0f,
        DeadBandUpperBoundary = 0.8,
        DeadBandLowerBoundary = 0.3,
        BelowToleranceLiftThickness = 0.05,
        AboveToleranceLiftThickness = 0.95,
        OverridingLiftThickness = 0.5
      };

      SimpleBinarizableInstanceTester.TestClass(liftParameters, "Custom lift parameters not same after round trip serialisation");
    }
  }
}
