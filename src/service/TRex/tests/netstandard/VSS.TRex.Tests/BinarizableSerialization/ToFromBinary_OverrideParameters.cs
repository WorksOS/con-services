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
        CMVRange = new CMVRangePercentageRecord(91, 108),
        OverrideMachineMDP = true,
        OverridingMachineMDP = 24,
        MDPRange = new MDPRangePercentageRecord (87, 123),
        OverridingTargetPassCountRange = new PassCountRangeRecord (3, 7),
        OverrideTargetPassCount = true,
        OverridingTemperatureWarningLevels = new TemperatureWarningLevelsRecord (250, 692),
        OverrideTemperatureWarningLevels = true,
        TargetMachineSpeed = new MachineSpeedExtendedRecord (12, 163),
      };

      SimpleBinarizableInstanceTester.TestClass(overrides, "Custom override parameters not same after round trip serialisation");
    }
  }
}
