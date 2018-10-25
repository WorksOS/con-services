using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class AutoMapperTests
  {
    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }
    
    [TestMethod]
    public void MapProjectSettingsToCMVSettings()
    {
      // The useDefaultTargetRangeCmvPercent is set to "false".
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetCmv: false, customTargetCmv: 50, useDefaultTargetRangeCmvPercent: false, customTargetCmvPercentMinimum: 30, customTargetCmvPercentMaximum: 140
      );

      var cmv = AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetCmv, cmv.OverrideTargetCMV, "overrideTargetCMV not mapped correctly");
      Assert.AreEqual(ps.CustomTargetCmv, cmv.CmvTarget, "cmvTarget not mapped correctly");
      Assert.AreEqual(ps.CmvMinimum, cmv.MinCMV, "minCMV not mapped correctly");
      Assert.AreEqual(ps.CmvMaximum, cmv.MaxCMV, "maxCMV not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, cmv.MinCMVPercent, "minCMVPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, cmv.MaxCMVPercent, "maxCMVPercent not mapped correctly");

      // The useDefaultTargetRangeCmvPercent is set to "true".
      ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetCmv: false, customTargetCmv: 50, useDefaultTargetRangeCmvPercent: true, customTargetCmvPercentMinimum: 30, customTargetCmvPercentMaximum: 140
      );

      cmv = AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetCmv, cmv.OverrideTargetCMV, "overrideTargetCMV not mapped correctly");
      Assert.AreEqual(ps.CustomTargetCmv, cmv.CmvTarget, "cmvTarget not mapped correctly");
      Assert.AreEqual(ps.CmvMinimum, cmv.MinCMV, "minCMV not mapped correctly");
      Assert.AreEqual(ps.CmvMaximum, cmv.MaxCMV, "maxCMV not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum, cmv.MinCMVPercent, "minCMVPercent not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum, cmv.MaxCMVPercent, "maxCMVPercent not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToCMVSettingsEx()
    {
      // The useDefaultCMVTargets is set to "false".
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetCmv: false, customTargetCmv: 50, 
        useDefaultTargetRangeCmvPercent: false, customTargetCmvPercentMinimum: 30, customTargetCmvPercentMaximum: 140,
        useDefaultCMVTargets: false, customCMVTargets: new List<int> { 0, 40, 80, 120, 170 });

      var cmv = AutoMapperUtility.Automapper.Map<CMVSettingsEx>(ps);
      Assert.AreNotEqual(ps.useMachineTargetCmv, cmv.OverrideTargetCMV, "overrideTargetCMV not mapped correctly");
      Assert.AreEqual(ps.CustomTargetCmv, cmv.CmvTarget, "cmvTarget not mapped correctly");
      Assert.AreEqual(ps.CmvMinimum, cmv.MinCMV, "minCMV not mapped correctly");
      Assert.AreEqual(ps.CmvMaximum, cmv.MaxCMV, "maxCMV not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, cmv.MinCMVPercent, "minCMVPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, cmv.MaxCMVPercent, "maxCMVPercent not mapped correctly");
      Assert.AreNotEqual(ps.customCMVTargets, cmv.CustomCMVDetailTargets, "customCMVDetailTargets not mapped correctly");

      // The useDefaultCMVTargets is set to "true".
      ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetCmv: false, customTargetCmv: 50,
        useDefaultTargetRangeCmvPercent: false, customTargetCmvPercentMinimum: 30, customTargetCmvPercentMaximum: 140,
        useDefaultCMVTargets: true, customCMVTargets: new List<int> { 0, 40, 80, 120, 170 });

      cmv = AutoMapperUtility.Automapper.Map<CMVSettingsEx>(ps);
      Assert.AreNotEqual(ps.useMachineTargetCmv, cmv.OverrideTargetCMV, "overrideTargetCMV not mapped correctly");
      Assert.AreEqual(ps.CustomTargetCmv, cmv.CmvTarget, "cmvTarget not mapped correctly");
      Assert.AreEqual(ps.CmvMinimum, cmv.MinCMV, "minCMV not mapped correctly");
      Assert.AreEqual(ps.CmvMaximum, cmv.MaxCMV, "maxCMV not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, cmv.MinCMVPercent, "minCMVPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, cmv.MaxCMVPercent, "maxCMVPercent not mapped correctly");
      Assert.AreNotEqual(CompactionProjectSettings.DefaultSettings.customCMVTargets, cmv.CustomCMVDetailTargets, "customCMVDetailTargets not mapped correctly");

    }

    [TestMethod]
    public void MapProjectSettingsToMDPSettings()
    {
      // The useDefaultTargetRangeMdpPercent is set to "false".
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetMdp: false, customTargetMdp: 50, useDefaultTargetRangeMdpPercent: false, customTargetMdpPercentMinimum: 30, customTargetMdpPercentMaximum: 140
      );

      var mdp = AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetMdp, mdp.OverrideTargetMDP, "overrideTargetMDP not mapped correctly");
      Assert.AreEqual(ps.CustomTargetMdp, mdp.MdpTarget, "mdpTarget not mapped correctly");
      Assert.AreEqual(ps.MdpMinimum, mdp.MinMDP, "minMDP not mapped correctly");
      Assert.AreEqual(ps.MdpMaximum, mdp.MaxMDP, "maxMDP not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMinimum, mdp.MinMDPPercent, "minMDPPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMaximum, mdp.MaxMDPPercent, "maxMDPPercent not mapped correctly");

      // The useDefaultTargetRangeMdpPercent is set to "true".
      ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetMdp: false, customTargetMdp: 50, useDefaultTargetRangeMdpPercent: true, customTargetMdpPercentMinimum: 30, customTargetMdpPercentMaximum: 140
      );

      mdp = AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetMdp, mdp.OverrideTargetMDP, "overrideTargetMDP not mapped correctly");
      Assert.AreEqual(ps.CustomTargetMdp, mdp.MdpTarget, "mdpTarget not mapped correctly");
      Assert.AreEqual(ps.MdpMinimum, mdp.MinMDP, "minMDP not mapped correctly");
      Assert.AreEqual(ps.MdpMaximum, mdp.MaxMDP, "maxMDP not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum, mdp.MinMDPPercent, "minMDPPercent not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum, mdp.MaxMDPPercent, "maxMDPPercent not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToTemperatureSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetTemperature: false, customTargetTemperatureMinimum: 50, customTargetTemperatureMaximum: 140
      );

      var temp = AutoMapperUtility.Automapper.Map<TemperatureSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetTemperature, temp.OverrideTemperatureRange, "overrideTemperatureRange not mapped correctly");
      Assert.AreEqual(ps.customTargetTemperatureMinimum, temp.MinTemperature, "minTemperature not mapped correctly");
      Assert.AreEqual(ps.customTargetTemperatureMaximum, temp.MaxTemperature, "maxTemperature not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToTemperatureDetailsSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useDefaultTemperatureTargets: false, customTemperatureTargets: new List<double> { 0, 75, 180, 230, 290, 310, 320 }
      );

      var temp = AutoMapperUtility.Automapper.Map<TemperatureDetailsSettings>(ps);
      Assert.AreEqual(ps.customTemperatureTargets.Count, temp.CustomTemperatureDetailsTargets.Length, "Temperature total not mapped correctly");
      for (int i = 0; i < temp.CustomTemperatureDetailsTargets.Length; i++)
      {
        //Values are mapped to what Raptor expects i.e. 10ths of degrees
        Assert.AreEqual(ps.customTemperatureTargets[i], temp.CustomTemperatureDetailsTargets[i]/10, $"Temperature item {i} not mapped correctly");
      }
    }


    [TestMethod]
    public void MapProjectSettingsToPassCountSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useDefaultPassCountTargets: false, customPassCountTargets: new List<int> { 1, 2, 3, 5, 7, 9, 12, 15 }
      );

      var pc = AutoMapperUtility.Automapper.Map<PassCountSettings>(ps);
      Assert.AreEqual(ps.customPassCountTargets.Count, pc.passCounts.Length, "passCounts total not mapped correctly");
      for (int i = 0; i < pc.passCounts.Length; i++)
      {
        Assert.AreEqual(ps.customPassCountTargets[i], pc.passCounts[i], $"passCounts item {i} not mapped correctly");
      }
    }

    [TestMethod]
    public void MapProjectSettingsToCMVPercentChangeSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings();
      double[] expectedPercents = ps.CmvPercentChange;

      var cmvChange = AutoMapperUtility.Automapper.Map<CmvPercentChangeSettings>(ps);
      Assert.AreEqual(expectedPercents.Length, cmvChange.percents.Length, "percents total not mapped correctly");
      for (int i = 0; i < cmvChange.percents.Length; i++)
      {
        Assert.AreEqual(expectedPercents[i], cmvChange.percents[i], $"percents item {i} not mapped correctly");
      }
    }

    [TestMethod]
    public void MapProjectSettingsToCutFillSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useDefaultCutFillTolerances: false, customCutFillTolerances: new List<double> { 0.3, 0.2, 0.1, 0, -0.1, -0.2, -0.3 }
      );

      var cutFill = AutoMapperUtility.Automapper.Map<CutFillSettings>(ps);
      Assert.AreEqual(7, cutFill.percents.Length, "cutFill total not mapped correctly");
      double[] expectedPercents = ps.CustomCutFillTolerances;
      for (int i = 0; i < cutFill.percents.Length; i++)
      {
        Assert.AreEqual(expectedPercents[i], cutFill.percents[i], $"cutFill item {i} not mapped correctly");
      }
    }

    [TestMethod]
    public void MapProjectSettingsToCustomLiftBuildSettings()
    {
      // The useDefaultTargetRangeCmvPercent and useDefaultTargetRangeMdpPercent are set to "false".
      var ps = CompactionProjectSettings.CreateProjectSettings(false, 3, 11, false, 35, 129, false, 43, false, 44, false, 55, 103, false, 56, 102, false, 4, 8, null, null, null, null, null, false, new List<int> { 1, 2, 3, 5, 7, 9, 12, 16 });

      var lbs = AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
      Assert.IsNotNull(lbs.CCVRange, "cCVRange should not be null");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, lbs.CCVRange.Min, "cCVRange.Min not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, lbs.CCVRange.Max, "cCVRange.Max not mapped correctly");
      Assert.AreEqual(LiftDetectionType.None, lbs.LiftDetectionType, "liftDetectionType not mapped correctly");
      Assert.AreEqual(LiftThicknessType.Compacted, lbs.LiftThicknessType, "liftThicknessType not mapped correctly");
      Assert.IsNotNull(lbs.MDPRange, "mDPRange should not be null");
      Assert.AreEqual(ps.customTargetMdpPercentMinimum, lbs.MDPRange.Min, "mDPRange.Min not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMaximum, lbs.MDPRange.Max, "mDPRange.Max not mapped correctly");
      Assert.AreEqual(ps.NullableCustomTargetCmv, lbs.OverridingMachineCCV, "overridingMachineCCV not mapped correctly");
      Assert.AreEqual(ps.NullableCustomTargetMdp, lbs.OverridingMachineMDP, "overridingMachineMDP not mapped correctly");
      Assert.IsNotNull(lbs.OverridingTargetPassCountRange, "overridingTargetPassCountRange should not be null");
      Assert.AreEqual(ps.customTargetPassCountMinimum, lbs.OverridingTargetPassCountRange.Min, "overridingTargetPassCountRange.Min not mapped correctly");
      Assert.AreEqual(ps.customTargetPassCountMaximum, lbs.OverridingTargetPassCountRange.Max, "overridingTargetPassCountRange.Max not mapped correctly");
      Assert.IsNotNull(lbs.OverridingTemperatureWarningLevels, "overridingTemperatureWarningLevels should not be null");
      Assert.AreEqual(ps.CustomTargetTemperatureWarningLevelMinimum, lbs.OverridingTemperatureWarningLevels.Min, "overridingTemperatureWarningLevels.Min not mapped correctly");
      Assert.AreEqual(ps.CustomTargetTemperatureWarningLevelMaximum, lbs.OverridingTemperatureWarningLevels.Max, "overridingTemperatureWarningLevels.Max not mapped correctly");
      Assert.IsNotNull(lbs.MachineSpeedTarget, "machineSpeedTarget should not be null");
      Assert.AreEqual(ps.CustomTargetSpeedMinimum, lbs.MachineSpeedTarget.MinTargetMachineSpeed, "machineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(ps.CustomTargetSpeedMaximum, lbs.MachineSpeedTarget.MaxTargetMachineSpeed, "machineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");

      // The useDefaultTargetRangeCmvPercent and useDefaultTargetRangeMdpPercent are set to "true".
      ps = CompactionProjectSettings.CreateProjectSettings(false, 3, 11, false, 35, 129, false, 43, false, 44, true, 55, 103, true, 56, 102, false, 4, 8, null, null, null, null, null, false, new List<int> { 1, 2, 3, 5, 7, 9, 12, 16 });

      lbs = AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
      Assert.IsNotNull(lbs.CCVRange, "cCVRange should not be null");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum, lbs.CCVRange.Min, "cCVRange.Min not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum, lbs.CCVRange.Max, "cCVRange.Max not mapped correctly");
      Assert.AreEqual(LiftDetectionType.None, lbs.LiftDetectionType, "liftDetectionType not mapped correctly");
      Assert.AreEqual(LiftThicknessType.Compacted, lbs.LiftThicknessType, "liftThicknessType not mapped correctly");
      Assert.IsNotNull(lbs.MDPRange, "mDPRange should not be null");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum, lbs.MDPRange.Min, "mDPRange.Min not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum, lbs.MDPRange.Max, "mDPRange.Max not mapped correctly");
      Assert.AreEqual(ps.NullableCustomTargetCmv, lbs.OverridingMachineCCV, "overridingMachineCCV not mapped correctly");
      Assert.AreEqual(ps.NullableCustomTargetMdp, lbs.OverridingMachineMDP, "overridingMachineMDP not mapped correctly");
      Assert.IsNotNull(lbs.OverridingTargetPassCountRange, "overridingTargetPassCountRange should not be null");
      Assert.AreEqual(ps.customTargetPassCountMinimum, lbs.OverridingTargetPassCountRange.Min, "overridingTargetPassCountRange.Min not mapped correctly");
      Assert.AreEqual(ps.customTargetPassCountMaximum, lbs.OverridingTargetPassCountRange.Max, "overridingTargetPassCountRange.Max not mapped correctly");
      Assert.IsNotNull(lbs.OverridingTemperatureWarningLevels, "overridingTemperatureWarningLevels should not be null");
      Assert.AreEqual(ps.CustomTargetTemperatureWarningLevelMinimum, lbs.OverridingTemperatureWarningLevels.Min, "overridingTemperatureWarningLevels.Min not mapped correctly");
      Assert.AreEqual(ps.CustomTargetTemperatureWarningLevelMaximum, lbs.OverridingTemperatureWarningLevels.Max, "overridingTemperatureWarningLevels.Max not mapped correctly");
      Assert.IsNotNull(lbs.MachineSpeedTarget, "machineSpeedTarget should not be null");
      Assert.AreEqual(ps.CustomTargetSpeedMinimum, lbs.MachineSpeedTarget.MinTargetMachineSpeed, "machineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(ps.CustomTargetSpeedMaximum, lbs.MachineSpeedTarget.MaxTargetMachineSpeed, "machineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToDefaultLiftBuildSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(true, 3, 11, true, 35, 129, true, 43, true, 44, true, 55, 103, true, 56, 102, true, 4, 8, null, null, null, null, null, true, new List<int> { 1, 2, 3, 5, 7, 9, 12, 16 });

      var lbs = AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
      Assert.IsNotNull(lbs.CCVRange, "cCVRange should not be null");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMinimum, lbs.CCVRange.Min, "cCVRange.Min not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetCmvPercentMaximum, lbs.CCVRange.Max, "cCVRange.Max not mapped correctly");
      Assert.AreEqual(LiftDetectionType.None, lbs.LiftDetectionType, "liftDetectionType not mapped correctly");
      Assert.AreEqual(LiftThicknessType.Compacted, lbs.LiftThicknessType, "liftThicknessType not mapped correctly");
      Assert.IsNotNull(lbs.MDPRange, "mDPRange should not be null");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMinimum, lbs.MDPRange.Min, "mDPRange.Min not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.customTargetMdpPercentMaximum, lbs.MDPRange.Max, "mDPRange.Max not mapped correctly");
      Assert.IsNull(lbs.OverridingMachineCCV, "overridingMachineCCV should be null");
      Assert.IsNull(lbs.OverridingMachineMDP, "overridingMachineMDP should be null");
      Assert.IsNull(lbs.OverridingTargetPassCountRange, "overridingTargetPassCountRange should be null");
      Assert.IsNull(lbs.OverridingTemperatureWarningLevels, "overridingTemperatureWarningLevels should be null");
      Assert.IsNotNull(lbs.MachineSpeedTarget, "machineSpeedTarget should not be null");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.CustomTargetSpeedMinimum, lbs.MachineSpeedTarget.MinTargetMachineSpeed, "machineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(CompactionProjectSettings.DefaultSettings.CustomTargetSpeedMaximum, lbs.MachineSpeedTarget.MaxTargetMachineSpeed, "machineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");
    }
  }
}
