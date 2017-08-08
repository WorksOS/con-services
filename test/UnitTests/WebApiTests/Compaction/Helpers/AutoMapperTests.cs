using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.Productivity3D.WebApiModels.Report.Models;

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
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetCmv: false, customTargetCmv: 50, useDefaultTargetRangeCmvPercent: false, customTargetCmvPercentMinimum: 30, customTargetCmvPercentMaximum: 140
      );

      var cmv = AutoMapperUtility.Automapper.Map<CMVSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetCmv, cmv.overrideTargetCMV, "overrideTargetCMV not mapped correctly");
      Assert.AreEqual(ps.customTargetCmv*10, cmv.cmvTarget, "cmvTarget not mapped correctly");
      Assert.AreEqual(AutoMapperUtility.MIN_CMV_MDP_VALUE, cmv.minCMV, "minCMV not mapped correctly");
      Assert.AreEqual(AutoMapperUtility.MAX_CMV_MDP_VALUE, cmv.maxCMV, "maxCMV not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, cmv.minCMVPercent, "minCMVPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, cmv.maxCMVPercent, "maxCMVPercent not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToMDPSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetMdp: false, customTargetMdp: 50, useDefaultTargetRangeMdpPercent: false, customTargetMdpPercentMinimum: 30, customTargetMdpPercentMaximum: 140
      );

      var mdp = AutoMapperUtility.Automapper.Map<MDPSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetMdp, mdp.overrideTargetMDP, "overrideTargetMDP not mapped correctly");
      Assert.AreEqual(ps.customTargetMdp * 10, mdp.mdpTarget, "mdpTarget not mapped correctly");
      Assert.AreEqual(AutoMapperUtility.MIN_CMV_MDP_VALUE, mdp.minMDP, "minMDP not mapped correctly");
      Assert.AreEqual(AutoMapperUtility.MAX_CMV_MDP_VALUE, mdp.maxMDP, "maxMDP not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMinimum, mdp.minMDPPercent, "minMDPPercent not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMaximum, mdp.maxMDPPercent, "maxMDPPercent not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToTemperatureSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useMachineTargetTemperature: false, customTargetTemperatureMinimum: 50, customTargetTemperatureMaximum: 140
      );

      var temp = AutoMapperUtility.Automapper.Map<TemperatureSettings>(ps);
      Assert.AreNotEqual(ps.useMachineTargetTemperature, temp.overrideTemperatureRange, "overrideTemperatureRange not mapped correctly");
      Assert.AreEqual(ps.customTargetTemperatureMinimum, temp.minTemperature, "minTemperature not mapped correctly");
      Assert.AreEqual(ps.customTargetTemperatureMaximum, temp.maxTemperature, "maxTemperature not mapped correctly");
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

      var cmvChange = AutoMapperUtility.Automapper.Map<CmvPercentChangeSettings>(ps);
      Assert.AreEqual(3, cmvChange.percents.Length, "percents total not mapped correctly");
      double[] expectedPercents = new double[] { 5, 20, 50 };
      for (int i = 0; i < cmvChange.percents.Length; i++)
      {
        Assert.AreEqual(expectedPercents[i], cmvChange.percents[i], $"percents item {i} not mapped correctly");
      }

    }

    [TestMethod]
    public void MapProjectSettingsToCustomLiftBuildSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(false, 3, 11, false, 35, 129, false, 43, false, 44, false, 55, 103, false, 56, 102, false, 4, 8, null, null, null, null, null, false, new List<int> { 1, 2, 3, 5, 7, 9, 12, 16 });

      var lbs = AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
      Assert.IsNotNull(lbs.cCVRange, "cCVRange should not be null");
      Assert.AreEqual(ps.customTargetCmvPercentMinimum, lbs.cCVRange.min, "cCVRange.min not mapped correctly");
      Assert.AreEqual(ps.customTargetCmvPercentMaximum, lbs.cCVRange.max, "cCVRange.max not mapped correctly");
      Assert.AreEqual(LiftDetectionType.None, lbs.liftDetectionType, "liftDetectionType not mapped correctly");
      Assert.AreEqual(LiftThicknessType.Compacted, lbs.liftThicknessType, "liftThicknessType not mapped correctly");
      Assert.IsNotNull(lbs.mDPRange, "mDPRange should not be null");
      Assert.AreEqual(ps.customTargetMdpPercentMinimum, lbs.mDPRange.min, "mDPRange.min not mapped correctly");
      Assert.AreEqual(ps.customTargetMdpPercentMaximum, lbs.mDPRange.max, "mDPRange.max not mapped correctly");
      Assert.AreEqual(ps.customTargetCmv * 10, lbs.overridingMachineCCV, "overridingMachineCCV not mapped correctly");
      Assert.AreEqual(ps.customTargetMdp * 10, lbs.overridingMachineMDP, "overridingMachineMDP not mapped correctly");
      Assert.IsNotNull(lbs.overridingTargetPassCountRange, "overridingTargetPassCountRange should not be null");
      Assert.AreEqual(ps.customTargetPassCountMinimum, lbs.overridingTargetPassCountRange.min, "overridingTargetPassCountRange.min not mapped correctly");
      Assert.AreEqual(ps.customTargetPassCountMaximum, lbs.overridingTargetPassCountRange.max, "overridingTargetPassCountRange.max not mapped correctly");
      Assert.IsNotNull(lbs.overridingTemperatureWarningLevels, "overridingTemperatureWarningLevels should not be null");
      Assert.AreEqual(Math.Round(ps.customTargetTemperatureMinimum.Value * 10), lbs.overridingTemperatureWarningLevels.min, "overridingTemperatureWarningLevels.min not mapped correctly");
      Assert.AreEqual(Math.Round(ps.customTargetTemperatureMaximum.Value * 10), lbs.overridingTemperatureWarningLevels.max, "overridingTemperatureWarningLevels.max not mapped correctly");
      Assert.IsNotNull(lbs.machineSpeedTarget, "machineSpeedTarget should not be null");
      Assert.AreEqual(Math.Round(ps.customTargetSpeedMinimum.Value * ConversionConstants.KM_HR_TO_CM_SEC), lbs.machineSpeedTarget.MinTargetMachineSpeed, "machineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(Math.Round(ps.customTargetSpeedMaximum.Value * ConversionConstants.KM_HR_TO_CM_SEC), lbs.machineSpeedTarget.MaxTargetMachineSpeed, "machineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");
    }

    [TestMethod]
    public void MapProjectSettingsToDefaultLiftBuildSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(true, 3, 11, true, 35, 129, true, 43, true, 44, true, 55, 103, true, 56, 102, true, 4, 8, null, null, null, null, null, true, new List<int> { 1, 2, 3, 5, 7, 9, 12, 16 });

      var lbs = AutoMapperUtility.Automapper.Map<LiftBuildSettings>(ps);
      Assert.IsNull(lbs.cCVRange, "cCVRange should be null");
      Assert.AreEqual(LiftDetectionType.None, lbs.liftDetectionType, "liftDetectionType not mapped correctly");
      Assert.AreEqual(LiftThicknessType.Compacted, lbs.liftThicknessType, "liftThicknessType not mapped correctly");
      Assert.IsNull(lbs.mDPRange, "mDPRange should be null");
      Assert.IsNull(lbs.overridingMachineCCV, "overridingMachineCCV should be null");
      Assert.IsNull(lbs.overridingMachineMDP, "overridingMachineMDP should be null");
      Assert.IsNull(lbs.overridingTargetPassCountRange, "overridingTargetPassCountRange should be null");
      Assert.IsNull(lbs.overridingTemperatureWarningLevels, "overridingTemperatureWarningLevels should be null");
      Assert.IsNotNull(lbs.machineSpeedTarget, "machineSpeedTarget should not be null");
      Assert.AreEqual(Math.Round(CompactionProjectSettings.DefaultSettings.customTargetSpeedMinimum.Value * ConversionConstants.KM_HR_TO_CM_SEC), lbs.machineSpeedTarget.MinTargetMachineSpeed, "machineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(Math.Round(CompactionProjectSettings.DefaultSettings.customTargetSpeedMaximum.Value * ConversionConstants.KM_HR_TO_CM_SEC), lbs.machineSpeedTarget.MaxTargetMachineSpeed, "machineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");
    }
  }
}
