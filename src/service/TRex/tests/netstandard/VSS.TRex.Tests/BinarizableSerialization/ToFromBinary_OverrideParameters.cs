using VSS.TRex.Common.Records;
using VSS.TRex.Profiling;
using VSS.TRex.Tests.BinarizableSerialization.Analytics;
using Xunit;

namespace VSS.TRex.Tests.BinarizableSerialization
{
  public class ToFromBinary_OverrideParameters : IClassFixture<AnalyticsTestsDIFixture>
  {
    [Fact]
    public void Test_OverrideParameters_Simple()
    {
      SimpleBinarizableInstanceTester.TestClass<OverrideParameters>("Empty override parameters not same after round trip serialisation");
    }

    [Fact]
    public void Test_OverrideParameters_Defaults()
    {
      var overrides = new OverrideParameters();
      SimpleBinarizableInstanceTester.TestClass(overrides, "Default override parameters not same after round trip serialisation");
    }

    [Fact]
    public void Test_OverrideParameters_Custom()
    {
      var overrides = new OverrideParameters
      {
        OverrideMachineCCV = true,
        OverridingMachineCCV = 72,
        CMVRange = new CMVRangePercentageRecord { Min = 91, Max = 108 },
        OverrideMachineMDP = true,
        OverridingMachineMDP = 24,
        MDPRange = new MDPRangePercentageRecord { Min = 87, Max = 123 },
        OverridingTargetPassCountRange = new PassCountRangeRecord { Min = 3, Max = 7 },
        OverrideTargetPassCount = true,
        OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord { Min = 250, Max = 692 },
        OverrideTemperatureWarningLevels = true,
        TargetMachineSpeed = new MachineSpeedExtendedRecord { Min = 12, Max = 163 },
      };

      SimpleBinarizableInstanceTester.TestClass(overrides, "Custom override parameters not same after round trip serialisation");
    }
  }
}
