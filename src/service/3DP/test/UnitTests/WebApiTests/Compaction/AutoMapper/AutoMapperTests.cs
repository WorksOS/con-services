using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Models.Reports;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.AutoMapper
{
  [TestClass]
  public class AutoMapperTests
  {
    private CellPassesV2Result GetCellPassesV2ResultWithNullValues()
    {
      var filteredPass = new CellPassesV2Result.CellPassValue()
      {
        Amplitude = ushort.MaxValue,
        Ccv = short.MaxValue,
        Frequency = ushort.MaxValue,
        GpsModeStore = byte.MaxValue,
        Height = float.MaxValue,
        MachineId = long.MaxValue,
        MachineSpeed = ushort.MaxValue,
        MaterialTemperature = ushort.MaxValue,
        Mdp = short.MaxValue,
        RadioLatency = byte.MaxValue,
        Time = DateTime.MinValue,
        Rmv = short.MaxValue
      };

      var eventsValue = new CellPassesV2Result.CellEventsValue()
      {
        EventMachineRmvThreshold = short.MaxValue,
        EventAutoVibrationState = AutoStateType.Auto,
        EventDesignNameId = 456,
        EventIcFlags = 5,
        EventInAvoidZoneState = 3,
        EventMachineAutomatics = GCSAutomaticsModeType.Automatic,
        EventMachineGear = MachineGearType.Forward,
        EventMinElevMapping = 0,
        EventOnGroundState = OnGroundStateType.YesLegacy,
        EventVibrationState = VibrationStateType.On,
        LayerId = ushort.MaxValue,
        GpsAccuracy = GPSAccuracyType.Fine,
        MapResetPriorDate = DateTime.MinValue,
        GpsTolerance = 3,
        MapResetDesignNameId = 1,
        PositioningTech = PositioningTechType.GPS
      };

      var targetsValue = new CellPassesV2Result.CellTargetsValue()
      {
        TargetThickness = float.MaxValue,
        TargetCcv = short.MaxValue,
        TargetPassCount = ushort.MaxValue,
        TempWarningLevelMin = ushort.MaxValue,
        TempWarningLevelMax = ushort.MaxValue,
        TargetMdp = short.MaxValue
      };

      var passes = new[]
      {
        new CellPassesV2Result.FilteredPassData()
        {
          FilteredPass = filteredPass,
          EventsValue = eventsValue,
          TargetsValue = targetsValue
        }
      };

      var layers = new[]
      {
        new CellPassesV2Result.ProfileLayer()
        {
          Amplitude = ushort.MaxValue,
          Ccv = short.MaxValue,
          CcvElev = 525,
          CcvMachineId = 1,
          CcvTime = DateTime.MinValue,
          FilteredHalfPassCount = 1,
          FilteredPassCount = 1,
          FirstPassHeight = 445,
          Frequency = ushort.MaxValue,
          Height = 400,
          LastLayerPassTime = DateTime.MinValue,
          LastPassHeight = 525,
          MachineId = 1,
          MaterialTemperature = ushort.MaxValue,
          MaterialTemperatureElev = 500,
          MaterialTemperatureMachineId = 1,
          MaterialTemperatureTime = DateTime.MinValue,
          MaximumPassHeight = 525,
          MaxThickness = 1.5F,
          Mdp = short.MaxValue,
          MdpElev = 525,
          MdpMachineId = 1,
          MdpTime = DateTime.MinValue,
          MinimumPassHeight = 445,
          RadioLatency = 12,
          Rmv = short.MaxValue,
          TargetCcv = short.MaxValue,
          TargetMdp = short.MaxValue,
          TargetPassCount = 7,
          TargetThickness = float.MaxValue,
          Thickness = 15,
          PassData = passes
        }
      };

      return new CellPassesV2Result() {Layers = layers};
    }

    private CellPassesV2Result GetCellPassesV2Result()
    {
      var filteredPass = new CellPassesV2Result.CellPassValue()
      {
        Amplitude = 120,
        Ccv = 230,
        Frequency = 25,
        GpsModeStore = 3,
        Height = 400,
        MachineId = 1,
        MachineSpeed = 15,
        MaterialTemperature = 120,
        Mdp = 210,
        RadioLatency = 18,
        Time = DateTime.UtcNow,
        Rmv = 170
      };

      var eventsValue = new CellPassesV2Result.CellEventsValue()
      {
        EventMachineRmvThreshold = 200,
        EventAutoVibrationState = AutoStateType.Auto,
        EventDesignNameId = 456,
        EventIcFlags = 5,
        EventInAvoidZoneState = 3,
        EventMachineAutomatics = GCSAutomaticsModeType.Automatic,
        EventMachineGear = MachineGearType.Forward,
        EventMinElevMapping = 0,
        EventOnGroundState = OnGroundStateType.YesLegacy,
        EventVibrationState = VibrationStateType.On,
        LayerId = 1,
        GpsAccuracy = GPSAccuracyType.Fine,
        MapResetPriorDate = DateTime.UtcNow,
        GpsTolerance = 3,
        MapResetDesignNameId = 1,
        PositioningTech = PositioningTechType.GPS
      };

      var targetsValue = new CellPassesV2Result.CellTargetsValue()
      {
        TargetThickness = 10,
        TargetCcv = 250,
        TargetPassCount = 10,
        TempWarningLevelMin = 20,
        TempWarningLevelMax = 150,
        TargetMdp = 180
      };

      var passes = new[]
      {
        new CellPassesV2Result.FilteredPassData()
        {
          FilteredPass = filteredPass,
          EventsValue = eventsValue,
          TargetsValue = targetsValue
        }
      };

      var layers = new[]
      {
        new CellPassesV2Result.ProfileLayer()
        {
          Amplitude = 100,
          Ccv = 200,
          CcvElev = 525,
          CcvMachineId = 1,
          CcvTime = DateTime.UtcNow,
          FilteredHalfPassCount = 1,
          FilteredPassCount = 1,
          FirstPassHeight = 445,
          Frequency = 123,
          Height = 400,
          LastLayerPassTime = DateTime.UtcNow,
          LastPassHeight = 525,
          MachineId = 1,
          MaterialTemperature = 150,
          MaterialTemperatureElev = 500,
          MaterialTemperatureMachineId = 1,
          MaterialTemperatureTime = DateTime.UtcNow,
          MaximumPassHeight = 525,
          MaxThickness = 1.5F,
          Mdp = 250,
          MdpElev = 525,
          MdpMachineId = 1,
          MdpTime = DateTime.UtcNow,
          MinimumPassHeight = 445,
          RadioLatency = 12,
          Rmv = 120,
          TargetCcv = 500,
          TargetMdp = 300,
          TargetPassCount = 7,
          TargetThickness = 10,
          Thickness = 15,
          PassData = passes
        }
      };

      return new CellPassesV2Result() { Layers = layers };
    }

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
        useDefaultCMVTargets: false, customCMVTargets: new List<int>
        {
          0,
          40,
          80,
          120,
          170
        });

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
        useDefaultCMVTargets: true, customCMVTargets: new List<int>
        {
          0,
          40,
          80,
          120,
          170
        });

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
        useDefaultTemperatureTargets: false, customTemperatureTargets: new List<double>
        {
          0,
          75,
          180,
          230,
          290,
          310,
          320
        }
      );

      var temp = AutoMapperUtility.Automapper.Map<TemperatureDetailsSettings>(ps);
      Assert.AreEqual(ps.customTemperatureTargets.Count, temp.CustomTemperatureDetailsTargets.Length, "Temperature total not mapped correctly");
      for (int i = 0; i < temp.CustomTemperatureDetailsTargets.Length; i++)
      {
        //Values are mapped to what Raptor expects i.e. 10ths of degrees
        Assert.AreEqual(ps.customTemperatureTargets[i], temp.CustomTemperatureDetailsTargets[i] / 10, $"Temperature item {i} not mapped correctly");
      }
    }

    [TestMethod]
    public void MapProjectSettingsToPassCountSettings()
    {
      var ps = CompactionProjectSettings.CreateProjectSettings(
        useDefaultPassCountTargets: false, customPassCountTargets: new List<int>
        {
          1,
          2,
          3,
          5,
          7,
          9,
          12,
          15
        }
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
        useDefaultCutFillTolerances: false, customCutFillTolerances: new List<double>
        {
          0.3,
          0.2,
          0.1,
          0,
          -0.1,
          -0.2,
          -0.3
        }
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
      var ps = CompactionProjectSettings.CreateProjectSettings(false, 3, 11, false, 35, 129, false, 43, false, 44, false, 55, 103, false, 56, 102, false, 4, 8, null, null, null, null, null, false, new List<int>
      {
        1,
        2,
        3,
        5,
        7,
        9,
        12,
        16
      });

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
      ps = CompactionProjectSettings.CreateProjectSettings(false, 3, 11, false, 35, 129, false, 43, false, 44, true, 55, 103, true, 56, 102, false, 4, 8, null, null, null, null, null, false, new List<int>
      {
        1,
        2,
        3,
        5,
        7,
        9,
        12,
        16
      });

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
      var ps = CompactionProjectSettings.CreateProjectSettings(true, 3, 11, true, 35, 129, true, 43, true, 44, true, 55, 103, true, 56, 102, true, 4, 8, null, null, null, null, null, true, new List<int>
      {
        1,
        2,
        3,
        5,
        7,
        9,
        12,
        16
      });

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

    [TestMethod]
    [DataRow(null, "87e6bd66-54d8-4651-8907-88b15d81b2d7",
      null, -1, null,
      true, false, false, false, false, false,
      null,
      1.0, GridReportOption.Automatic,
      0.0, 0.0, 0.0, 0.0, 0.0)]
    [DataRow(null, "87e6bd66-54d8-4651-8907-88b15d81b2d7",
      null, -1, null,
      true, false, false, false, false, false,
      "57e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.0, GridReportOption.Automatic,
      0.0, 0.0, 0.0, 0.0, 0.0)]
    public void MapGridReportRequestToTRexRequest(
      long? projectId, string projectString,
      FilterResult filter, long filterId, LiftBuildSettings liftBuildSettings,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      string designFileString,
      double? gridInterval, GridReportOption gridReportOption,
      double startNorthing, double startEasting, double endNorthing, double endEasting, double azimuth)
    {
      Guid? projectUid = string.IsNullOrEmpty(projectString) ? (Guid?) null : Guid.Parse(projectString);
      var designFile = string.IsNullOrEmpty(designFileString)
        ? (DesignDescriptor) null
        : new DesignDescriptor(-1, null, -1, Guid.Parse(designFileString));
      var apiRequest = CompactionReportGridRequest.CreateCompactionReportGridRequest(
        projectId, projectUid,
        filter, filterId, liftBuildSettings,
        true, false, false, false, false, false,
        designFile,
        gridInterval, GridReportOption.Automatic,
        startNorthing, startEasting, endNorthing, endEasting, azimuth);
      apiRequest.Validate();

      var tRexRequest = AutoMapperUtility.Automapper.Map<CompactionReportGridTRexRequest>(apiRequest);
      Assert.AreEqual(projectUid, tRexRequest.ProjectUid, "projectUid not mapped correctly");
      Assert.AreEqual(apiRequest.Filter, tRexRequest.Filter, "Filter not mapped correctly");
      Assert.AreEqual(apiRequest.ReportElevation, tRexRequest.ReportElevation, "ReportElevation not mapped correctly");
      Assert.AreEqual(apiRequest.ReportCMV, tRexRequest.ReportCmv, "ReportCMV not mapped correctly");
      Assert.AreEqual(apiRequest.ReportMDP, tRexRequest.ReportMdp, "ReportMDP not mapped correctly");
      Assert.AreEqual(apiRequest.ReportTemperature, tRexRequest.ReportTemperature, "ReportTemperature not mapped correctly");
      Assert.AreEqual(apiRequest.ReportCutFill, tRexRequest.ReportCutFill, "ReportCutFill not mapped correctly");
      Assert.AreEqual(apiRequest.ReportElevation, tRexRequest.ReportElevation, "ReportElevation not mapped correctly");
      if (string.IsNullOrEmpty(designFileString))
        Assert.IsNull(tRexRequest.CutFillDesignUid, "CutFillDesignUid not mapped correctly");
      else
        Assert.AreEqual(designFileString, tRexRequest.CutFillDesignUid.ToString(), "CutFillDesignUid not mapped correctly");

      Assert.AreEqual(apiRequest.GridInterval, tRexRequest.GridInterval, "GridInterval not mapped correctly");
      Assert.AreEqual(apiRequest.GridReportOption, tRexRequest.GridReportOption, "GridReportOption not mapped correctly");
      Assert.AreEqual(apiRequest.StartNorthing, tRexRequest.StartNorthing, "StartNorthing not mapped correctly");
      Assert.AreEqual(apiRequest.StartEasting, tRexRequest.StartEasting, "StartEasting not mapped correctly");
      Assert.AreEqual(apiRequest.EndNorthing, tRexRequest.EndNorthing, "EndNorthing not mapped correctly");
      Assert.AreEqual(apiRequest.EndEasting, tRexRequest.EndEasting, "EndEasting not mapped correctly");
      Assert.AreEqual(apiRequest.Azimuth, tRexRequest.Azimuth, "Azimuth not mapped correctly");
    }

    [TestMethod]
    [DataRow(null, "87e6bd66-54d8-4651-8907-88b15d81b2d7",
      null, -1, null,
      true, false, false, false, false, false,
      null, "33e6bd66-54d8-4651-8907-88b15d81b2d7",
      2.0, 100.0, 200.0, new double[] {-1.0, -0.5, 0.0, 1.5})]
    [DataRow(null, "87e6bd66-54d8-4651-8907-88b15d81b2d7",
      null, -1, null,
      true, false, false, false, true, true,
      "57e6bd66-54d8-4651-8907-88b15d81b2d7", "33e6bd66-54d8-4651-8907-88b15d81b2d7",
      1.5, 50.0, 2000.0, new double[] {0.0})]
    public void MapStationOffsetReportRequestToTRexRequest(
      long? projectId, string projectString,
      FilterResult filter, long filterId, LiftBuildSettings liftBuildSettings,
      bool reportElevation, bool reportCmv, bool reportMdp, bool reportPassCount, bool reportTemperature, bool reportCutFill,
      string designFileString, string alignmentFileString,
      double crossSectionInterval, double startStation, double endStation, double[] offsets)
    {
      Guid? projectUid = string.IsNullOrEmpty(projectString) ? (Guid?) null : Guid.Parse(projectString);
      var cutFillDesignDescriptor = string.IsNullOrEmpty(designFileString)
        ? (DesignDescriptor) null
        : new DesignDescriptor(-1, null, -1, Guid.Parse(designFileString));

      var alignmentDescriptor = new DesignDescriptor(-1, null, -1, Guid.Parse(alignmentFileString));

      var apiRequest = CompactionReportStationOffsetRequest.CreateRequest(
        projectId, projectUid,
        filter, filterId, liftBuildSettings,
        true, false, false, false, false, false,
        cutFillDesignDescriptor, alignmentDescriptor,
        crossSectionInterval, startStation, endStation, offsets,
        null, string.Empty);
      apiRequest.Validate();

      var tRexRequest = AutoMapperUtility.Automapper.Map<CompactionReportStationOffsetTRexRequest>(apiRequest);
      Assert.AreEqual(projectUid, tRexRequest.ProjectUid, "projectUid not mapped correctly");
      Assert.AreEqual(apiRequest.Filter, tRexRequest.Filter, "Filter not mapped correctly");
      Assert.AreEqual(apiRequest.ReportElevation, tRexRequest.ReportElevation, "ReportElevation not mapped correctly");
      Assert.AreEqual(apiRequest.ReportCMV, tRexRequest.ReportCmv, "ReportCmv not mapped correctly");
      Assert.AreEqual(apiRequest.ReportMDP, tRexRequest.ReportMdp, "ReportMdp not mapped correctly");
      Assert.AreEqual(apiRequest.ReportTemperature, tRexRequest.ReportTemperature, "ReportTemperature not mapped correctly");
      Assert.AreEqual(apiRequest.ReportCutFill, tRexRequest.ReportCutFill, "ReportCutFill not mapped correctly");
      Assert.AreEqual(apiRequest.ReportElevation, tRexRequest.ReportElevation, "ReportElevation not mapped correctly");
      if (string.IsNullOrEmpty(designFileString))
        Assert.IsNull(tRexRequest.CutFillDesignUid, "CutFillDesignUid not mapped correctly");
      else
        Assert.AreEqual(designFileString, tRexRequest.CutFillDesignUid.ToString(), "CutFillDesignUid not mapped correctly");

      if (string.IsNullOrEmpty(alignmentFileString))
        Assert.IsNull(tRexRequest.AlignmentDesignUid, "AlignmentDesignUid not mapped correctly");
      else
        Assert.AreEqual(alignmentFileString, tRexRequest.AlignmentDesignUid.ToString(), "AlignmentDesignUid not mapped correctly");

      Assert.AreEqual(tRexRequest.CrossSectionInterval, apiRequest.CrossSectionInterval, "CrossSectionInterval not mapped correctly");
      Assert.AreEqual(tRexRequest.StartStation, apiRequest.StartStation, "StartStation not mapped correctly");
      Assert.AreEqual(tRexRequest.EndStation, apiRequest.EndStation, "EndStation not mapped correctly");
      Assert.AreEqual(tRexRequest.Offsets.Length, apiRequest.Offsets.Length, "Offset count not mapped correctly");
      Assert.AreEqual(tRexRequest.Offsets.Length > 0 ? tRexRequest.Offsets[0] : -6666,
        apiRequest.Offsets.Length > 0 ? apiRequest.Offsets[0] : -6666, "Offset[0] not mapped correctly");

    }

    [TestMethod]
    public void MapLiftBuildSettingsToOverridingTargets()
    {
      var lbs = new LiftBuildSettings(new CCVRangePercentage(70, 100), false, 0, 0, 0, LiftDetectionType.AutoMapReset,
        LiftThicknessType.Compacted, new MDPRangePercentage(80, 125), false, null, 70, 812, new TargetPassCountRange(3, 8),
        new TemperatureWarningLevels(1000, 1800), null, null, new MachineSpeedTarget(123, 456));

      var overrides = AutoMapperUtility.Automapper.Map<OverridingTargets>(lbs);

      Assert.AreEqual(lbs.CCVRange.Min, overrides.MinCMVPercent, "MinCMVPercent not mapped correctly");
      Assert.AreEqual(lbs.CCVRange.Max, overrides.MaxCMVPercent, "MaxCMVPercent not mapped correctly");
      Assert.AreEqual(true, overrides.OverrideTargetCMV, "OverrideTargetCMV not mapped correctly");
      Assert.AreEqual(lbs.OverridingMachineCCV, overrides.CmvTarget, "CmvTarget not mapped correctly");
      Assert.AreEqual(lbs.MDPRange.Min, overrides.MinMDPPercent, "MinMDPPercent not mapped correctly");
      Assert.AreEqual(lbs.MDPRange.Max, overrides.MaxMDPPercent, "MaxMDPPercent not mapped correctly");
      Assert.AreEqual(true, overrides.OverrideTargetMDP, "OverrideTargetMDP not mapped correctly");
      Assert.AreEqual(lbs.OverridingMachineMDP, overrides.MdpTarget, "MdpTarget not mapped correctly");
      Assert.IsNotNull(overrides.OverridingTargetPassCountRange, "OverridingTargetPassCountRange should not be null");
      Assert.AreEqual(lbs.OverridingTargetPassCountRange.Min, overrides.OverridingTargetPassCountRange.Min, "OverridingTargetPassCountRange.Min not mapped correctly");
      Assert.AreEqual(lbs.OverridingTargetPassCountRange.Max, overrides.OverridingTargetPassCountRange.Max, "OverridingTargetPassCountRange.Max not mapped correctly");
      Assert.IsNotNull(overrides.TemperatureSettings, "TemperatureSettings should not be null");
      Assert.AreEqual(lbs.OverridingTemperatureWarningLevels.Min / 10.0, overrides.TemperatureSettings.MinTemperature, "TemperatureSettings.MinTemperature not mapped correctly");
      Assert.AreEqual(lbs.OverridingTemperatureWarningLevels.Max / 10.0, overrides.TemperatureSettings.MaxTemperature, "TemperatureSettings.MaxTemperature not mapped correctly");
      Assert.IsNotNull(overrides.MachineSpeedTarget, "MachineSpeedTarget should not be null");
      Assert.AreEqual(lbs.MachineSpeedTarget.MinTargetMachineSpeed, overrides.MachineSpeedTarget.MinTargetMachineSpeed, "MachineSpeedTarget.MinTargetMachineSpeed not mapped correctly");
      Assert.AreEqual(lbs.MachineSpeedTarget.MaxTargetMachineSpeed, overrides.MachineSpeedTarget.MaxTargetMachineSpeed, "MachineSpeedTarget.MaxTargetMachineSpeed not mapped correctly");
    }

    [TestMethod]
    public void MapLiftBuildSettingsToLiftSettingsDefaults()
    {
      var lbs = new LiftBuildSettings(null, false, 0, 0, 0, LiftDetectionType.None,
        LiftThicknessType.Compacted, null, false, null, null, null, null,
        null, null, null, null);

      var settings = AutoMapperUtility.Automapper.Map<LiftSettings>(lbs);
      Assert.AreEqual(lbs.CCVSummarizeTopLayerOnly, settings.CCVSummarizeTopLayerOnly);
      Assert.AreEqual(lbs.MDPSummarizeTopLayerOnly, settings.MDPSummarizeTopLayerOnly);
      Assert.IsNull(settings.CCVSummaryType);
      Assert.IsNull(settings.MDPSummaryType);
      Assert.AreEqual(lbs.FirstPassThickness, settings.FirstPassThickness);
      Assert.AreEqual(lbs.LiftDetectionType, settings.LiftDetectionType);
      Assert.AreEqual(lbs.LiftThicknessType, settings.LiftThicknessType);
      Assert.AreEqual(lbs.LiftThicknessTarget, settings.LiftThicknessTarget);
      Assert.AreEqual(false, settings.OverrideMachineThickness);
      Assert.AreEqual(0, settings.OverridingLiftThickness);
      Assert.AreEqual(false, settings.IncludeSupersededLifts);
      Assert.AreEqual(lbs.DeadBandLowerBoundary, settings.DeadBandLowerBoundary);
      Assert.AreEqual(lbs.DeadBandUpperBoundary, settings.DeadBandUpperBoundary);
    }

    [TestMethod]
    public void MapLiftBuildSettingsToLiftSettingsCustom()
    {
      var lbs = new LiftBuildSettings(null, true, 0.7, 1.15, 0.2f, LiftDetectionType.Tagfile,
        LiftThicknessType.Compacted, null, true, 1.5f, null, null, null,
        null, true, new LiftThicknessTarget
        {
          TargetLiftThickness = 0.7f,
          AboveToleranceLiftThickness = 0.3f,
          BelowToleranceLiftThickness = 0.14f
        }, null);
      lbs.CCvSummaryType = CCVSummaryType.WorkInProgress; //this property is not in the constructor

      var settings = AutoMapperUtility.Automapper.Map<LiftSettings>(lbs);
      Assert.AreEqual(lbs.CCVSummarizeTopLayerOnly, settings.CCVSummarizeTopLayerOnly);
      Assert.AreEqual(lbs.MDPSummarizeTopLayerOnly, settings.MDPSummarizeTopLayerOnly);
      Assert.AreEqual((SummaryType) lbs.CCvSummaryType, settings.CCVSummaryType);
      Assert.IsNull(settings.MDPSummaryType);
      Assert.AreEqual(lbs.FirstPassThickness, settings.FirstPassThickness);
      Assert.AreEqual(lbs.LiftDetectionType, settings.LiftDetectionType);
      Assert.AreEqual(lbs.LiftThicknessType, settings.LiftThicknessType);
      Assert.IsNotNull(settings.LiftThicknessTarget);
      Assert.AreEqual(lbs.LiftThicknessTarget.TargetLiftThickness, settings.LiftThicknessTarget.TargetLiftThickness);
      Assert.AreEqual(lbs.LiftThicknessTarget.AboveToleranceLiftThickness, settings.LiftThicknessTarget.AboveToleranceLiftThickness);
      Assert.AreEqual(lbs.LiftThicknessTarget.BelowToleranceLiftThickness, settings.LiftThicknessTarget.BelowToleranceLiftThickness);
      Assert.AreEqual(lbs.OverridingLiftThickness.HasValue, settings.OverrideMachineThickness);
      Assert.AreEqual(lbs.OverridingLiftThickness, settings.OverridingLiftThickness);
      Assert.AreEqual(lbs.IncludeSupersededLifts, settings.IncludeSupersededLifts);
      Assert.AreEqual(lbs.DeadBandLowerBoundary, settings.DeadBandLowerBoundary);
      Assert.AreEqual(lbs.DeadBandUpperBoundary, settings.DeadBandUpperBoundary);
    }

    [TestMethod]
    public void MapCellPassesResultProfileLayerToCellPassesV2ResultProfileLayer()
    {
      var cpv2 = GetCellPassesV2Result();

      var pl = AutoMapperUtility.Automapper.Map<CellPassesResult.ProfileLayer>(cpv2.Layers[0]);

      // Profile Layer...
      Assert.AreEqual(cpv2.Layers.Length, 1);
      Assert.AreEqual(pl.Amplitude, cpv2.Layers[0].Amplitude);
      Assert.AreEqual(pl.CCV, cpv2.Layers[0].Ccv);
      Assert.AreEqual(pl.CCV_Elev, cpv2.Layers[0].CcvElev);
      Assert.AreEqual(pl.CCV_MachineID, cpv2.Layers[0].CcvMachineId);
      Assert.AreEqual(pl.CCV_Time, cpv2.Layers[0].CcvTime);
      Assert.AreEqual(pl.FilteredHalfPassCount, cpv2.Layers[0].FilteredHalfPassCount);
      Assert.AreEqual(pl.FilteredPassCount, cpv2.Layers[0].FilteredPassCount);
      Assert.AreEqual(pl.FirstPassHeight, cpv2.Layers[0].FirstPassHeight);
      Assert.AreEqual(pl.Frequency, cpv2.Layers[0].Frequency);
      Assert.AreEqual(pl.Height, cpv2.Layers[0].Height);
      Assert.AreEqual(pl.LastLayerPassTime, cpv2.Layers[0].LastLayerPassTime);
      Assert.AreEqual(pl.LastPassHeight, cpv2.Layers[0].LastPassHeight);
      Assert.AreEqual(pl.MachineID, cpv2.Layers[0].MachineId);
      Assert.AreEqual(pl.MaterialTemperature, cpv2.Layers[0].MaterialTemperature);
      Assert.AreEqual(pl.MaterialTemperature_Elev, cpv2.Layers[0].MaterialTemperatureElev);
      Assert.AreEqual(pl.MaterialTemperature_MachineID, cpv2.Layers[0].MaterialTemperatureMachineId);
      Assert.AreEqual(pl.MaterialTemperature_Time, cpv2.Layers[0].MaterialTemperatureTime);
      Assert.AreEqual(pl.MaximumPassHeight, cpv2.Layers[0].MaximumPassHeight);
      Assert.AreEqual(pl.MaxThickness, cpv2.Layers[0].MaxThickness);
      Assert.AreEqual(pl.MDP, cpv2.Layers[0].Mdp);
      Assert.AreEqual(pl.MDP_Elev, cpv2.Layers[0].MdpElev);
      Assert.AreEqual(pl.MDP_MachineID, cpv2.Layers[0].MdpMachineId);
      Assert.AreEqual(pl.MDP_Time, cpv2.Layers[0].MdpTime);
      Assert.AreEqual(pl.MinimumPassHeight, cpv2.Layers[0].MinimumPassHeight);
      Assert.AreEqual(pl.RadioLatency, cpv2.Layers[0].RadioLatency);
      Assert.AreEqual(pl.RMV, cpv2.Layers[0].Rmv);
      Assert.AreEqual(pl.TargetCCV, cpv2.Layers[0].TargetCcv);
      Assert.AreEqual(pl.TargetMDP, cpv2.Layers[0].TargetMdp);
      Assert.AreEqual(pl.TargetPassCount, cpv2.Layers[0].TargetPassCount);
      Assert.AreEqual(pl.TargetThickness, cpv2.Layers[0].TargetThickness);
      Assert.AreEqual(pl.Thickness, cpv2.Layers[0].Thickness);
      // Filtered Pass Data...
      Assert.AreEqual(pl.FilteredPassData.Length, cpv2.Layers[0].PassData.Length);
      Assert.AreEqual(cpv2.Layers[0].PassData.Length, 1);
      // ------ Filtered Pass -------
      var fp = pl.FilteredPassData[0].FilteredPass;
      var fp2 = cpv2.Layers[0].PassData[0].FilteredPass;

      Assert.AreEqual(fp.Amplitude, fp2.Amplitude);
      Assert.AreEqual(fp.CCV, fp2.Ccv);
      Assert.AreEqual(fp.Frequency, fp2.Frequency);
      Assert.AreEqual(fp.Height, fp2.Height);
      Assert.AreEqual(fp.MachineID, fp2.MachineId);
      Assert.AreEqual(fp.MachineSpeed, fp2.MachineSpeed);
      Assert.AreEqual(fp.MaterialTemperature, fp2.MaterialTemperature);
      Assert.AreEqual(fp.MDP, fp2.Mdp);
      Assert.AreEqual(fp.RadioLatency, fp2.RadioLatency);
      Assert.AreEqual(fp.RMV, fp2.Rmv);
      Assert.AreEqual(fp.Time, fp2.Time);
      Assert.AreEqual(fp.GPSModeStore, fp2.GpsModeStore);
      // ------ Event Values -------
      var ev = pl.FilteredPassData[0].EventsValue;
      var ev2 = cpv2.Layers[0].PassData[0].EventsValue;

      Assert.AreEqual(ev.EventAutoVibrationState, ev2.EventAutoVibrationState);
      Assert.AreEqual(ev.EventDesignNameID, ev2.EventDesignNameId);
      Assert.AreEqual(ev.EventICFlags, ev2.EventIcFlags);
      Assert.AreEqual(ev.EventMachineAutomatics, ev2.EventMachineAutomatics);
      Assert.AreEqual(ev.EventMachineGear, ev2.EventMachineGear);
      Assert.AreEqual(ev.EventMachineRMVThreshold, ev2.EventMachineRmvThreshold);
      Assert.AreEqual(ev.EventOnGroundState, ev2.EventOnGroundState);
      Assert.AreEqual(ev.EventVibrationState, ev2.EventVibrationState);
      Assert.AreEqual(ev.GPSAccuracy, ev2.GpsAccuracy);
      Assert.AreEqual(ev.GPSTolerance, ev2.GpsTolerance);
      Assert.AreEqual(ev.LayerID, ev2.LayerId);
      Assert.AreEqual(ev.MapReset_DesignNameID, ev2.MapResetDesignNameId);
      Assert.AreEqual(ev.MapReset_PriorDate, ev2.MapResetPriorDate);
      Assert.AreEqual(ev.PositioningTech, ev2.PositioningTech);
      Assert.AreEqual(ev.EventInAvoidZoneState, ev2.EventInAvoidZoneState);
      Assert.AreEqual((byte) ev.EventMinElevMapping, ev2.EventMinElevMapping);
      // ------ Target Values -------
      var tv = pl.FilteredPassData[0].TargetsValue;
      var tv2 = cpv2.Layers[0].PassData[0].TargetsValue;
      Assert.AreEqual(tv.TargetCCV, tv2.TargetCcv);
      Assert.AreEqual(tv.TargetMDP, tv2.TargetMdp);
      Assert.AreEqual(tv.TargetPassCount, tv2.TargetPassCount);
      Assert.AreEqual(tv.TargetThickness, tv2.TargetThickness);
      Assert.AreEqual(tv.TempWarningLevelMin, tv2.TempWarningLevelMin);
      Assert.AreEqual(tv.TempWarningLevelMax, tv2.TempWarningLevelMax);
    }

    [TestMethod]
    public void CellPassesResult_JSON_Serialization()
    {
      var cpv2 = GetCellPassesV2Result();

      var pl = AutoMapperUtility.Automapper.Map<CellPassesResult.ProfileLayer>(cpv2.Layers[0]);

      var jsonString = JsonConvert.SerializeObject(pl);

      Assert.IsNotNull(jsonString);
      Assert.AreNotEqual(jsonString, string.Empty);

      var plObject = JsonConvert.DeserializeObject<JObject>(jsonString);

      Assert.IsNotNull(plObject);

      Assert.IsNotNull(plObject["amplitude"] != null);
      Assert.IsNotNull(plObject["cCV"] != null);
      Assert.IsNotNull(plObject["cCV_Elev"] != null);
      Assert.IsNotNull(plObject["cCV_MachineID"] != null);
      Assert.IsNotNull(plObject["cCV_Time"] != null);
      Assert.IsNotNull(plObject["filteredHalfPassCount"] != null);
      Assert.IsNotNull(plObject["filteredPassCount"] != null);
      Assert.IsNotNull(plObject["firstPassHeight"] != null);
      Assert.IsNotNull(plObject["frequency"] != null);
      Assert.IsNotNull(plObject["height"] != null);
      Assert.IsNotNull(plObject["lastLayerPassTime"] != null);
      Assert.IsNotNull(plObject["lastPassHeight"] != null);
      Assert.IsNotNull(plObject["machineID"] != null);
      Assert.IsNotNull(plObject["materialTemperature"] != null);
      Assert.IsNotNull(plObject["materialTemperature_Elev"] != null);
      Assert.IsNotNull(plObject["materialTemperature_MachineID"] != null);
      Assert.IsNotNull(plObject["materialTemperature_Time"] != null);
      Assert.IsNotNull(plObject["maximumPassHeight"] != null);
      Assert.IsNotNull(plObject["maxThickness"] != null);
      Assert.IsNotNull(plObject["mDP"] != null);
      Assert.IsNotNull(plObject["mDP_Elev"] != null);
      Assert.IsNotNull(plObject["mDP_MachineID"] != null);
      Assert.IsNotNull(plObject["mDP_Time"] != null);
      Assert.IsNotNull(plObject["minimumPassHeight"] != null);
      Assert.IsNotNull(plObject["radioLatency"] != null);
      Assert.IsNotNull(plObject["targetCCV"] != null);
      Assert.IsNotNull(plObject["targetMDP"] != null);
      Assert.IsNotNull(plObject["targetPassCount"] != null);
      Assert.IsNotNull(plObject["targetThickness"] != null);
      Assert.IsNotNull(plObject["thickness"] != null);
      Assert.IsNotNull(plObject["filteredPassData"] != null);

      var filteredPassData = plObject["filteredPassData"];

      var filteredPass = filteredPassData[0]["filteredPass"];

      Assert.IsNotNull(filteredPass != null);

      Assert.IsNotNull(filteredPass["amplitude"] != null);
      Assert.IsNotNull(filteredPass["cCV"] != null);
      Assert.IsNotNull(filteredPass["frequency"] != null);
      Assert.IsNotNull(filteredPass["height"] != null);
      Assert.IsNotNull(filteredPass["machineID"] != null);
      Assert.IsNotNull(filteredPass["machineSpeed"] != null);
      Assert.IsNotNull(filteredPass["materialTemperature"] != null);
      Assert.IsNotNull(filteredPass["mDP"] != null);
      Assert.IsNotNull(filteredPass["radioLatency"] != null);
      Assert.IsNotNull(filteredPass["rMV"] != null);
      Assert.IsNotNull(filteredPass["time"] != null);
      Assert.IsNotNull(filteredPass["gPSModeStore"] != null);

      var eventsValue = filteredPassData[0]["eventsValue"];

      Assert.IsNotNull(eventsValue != null);

      Assert.IsNotNull(eventsValue["eventAutoVibrationState"] != null);
      Assert.IsNotNull(eventsValue["eventDesignNameID"] != null);
      Assert.IsNotNull(eventsValue["eventICFlags"] != null);
      Assert.IsNotNull(eventsValue["eventMachineAutomatics"] != null);
      Assert.IsNotNull(eventsValue["eventMachineGear"] != null);
      Assert.IsNotNull(eventsValue["eventMachineRMVThreshold"] != null);
      Assert.IsNotNull(eventsValue["eventOnGroundState"] != null);
      Assert.IsNotNull(eventsValue["eventVibrationState"] != null);
      Assert.IsNotNull(eventsValue["gPSAccuracy"] != null);
      Assert.IsNotNull(eventsValue["gPSTolerance"] != null);
      Assert.IsNotNull(eventsValue["layerID"] != null);
      Assert.IsNotNull(eventsValue["mapReset_DesignNameID"] != null);
      Assert.IsNotNull(eventsValue["mapReset_PriorDate"] != null);
      Assert.IsNotNull(eventsValue["positioningTech"] != null);
      Assert.IsNotNull(eventsValue["EventInAvoidZoneState"] != null);
      Assert.IsNotNull(eventsValue["EventMinElevMapping"] != null);

      var targetsValue = filteredPassData[0]["targetsValue"];

      Assert.IsNotNull(targetsValue != null);

      Assert.IsNotNull(targetsValue["targetCCV"] != null);
      Assert.IsNotNull(targetsValue["targetMDP"] != null);
      Assert.IsNotNull(targetsValue["targetPassCount"] != null);
      Assert.IsNotNull(targetsValue["targetThickness"] != null);
      Assert.IsNotNull(targetsValue["tempWarningLevelMin"] != null);
      Assert.IsNotNull(targetsValue["tempWarningLevelMax"] != null);
    }

    [TestMethod]
    public void CellPassesResult_JSON_Serialization_Null_Values()
    {
      const float TOLERANCE = 0.000001F;

      var cpv2 = GetCellPassesV2ResultWithNullValues();

      var pl = AutoMapperUtility.Automapper.Map<CellPassesResult.ProfileLayer>(cpv2.Layers[0]);

      var jsonString = JsonConvert.SerializeObject(pl);

      Assert.IsNotNull(jsonString);
      Assert.AreNotEqual(jsonString, string.Empty);

      var plObject = JsonConvert.DeserializeObject<JObject>(jsonString);

      Assert.IsNotNull(plObject);

      Assert.AreEqual(plObject["amplitude"], ushort.MaxValue);
      Assert.AreEqual(plObject["cCV"], short.MaxValue);
      Assert.AreEqual(plObject["cCV_Time"], DateTime.MinValue);
      Assert.AreEqual(plObject["frequency"], ushort.MaxValue);
      Assert.AreEqual(plObject["lastLayerPassTime"], DateTime.MinValue);
      Assert.AreEqual(plObject["materialTemperature"], ushort.MaxValue);
      Assert.AreEqual(plObject["materialTemperature_Time"], DateTime.MinValue);
      Assert.AreEqual(plObject["mDP"], short.MaxValue);
      Assert.AreEqual(plObject["mDP_Time"], DateTime.MinValue);
      Assert.AreEqual(plObject["targetCCV"], short.MaxValue);
      Assert.AreEqual(plObject["targetMDP"], short.MaxValue);
      Assert.IsTrue(Math.Abs((float)plObject["targetThickness"] - float.MaxValue) < TOLERANCE);

      var filteredPassData = plObject["filteredPassData"];

      var filteredPass = filteredPassData[0]["filteredPass"];

      Assert.IsNotNull(filteredPass != null);

      Assert.AreEqual(filteredPass["amplitude"], ushort.MaxValue);
      Assert.AreEqual(filteredPass["cCV"], short.MaxValue);
      Assert.AreEqual(filteredPass["frequency"], ushort.MaxValue);
      Assert.AreEqual(filteredPass["gPSModeStore"], byte.MaxValue);
      Assert.IsTrue(Math.Abs((float)filteredPass["height"] - float.MaxValue) < TOLERANCE);
      Assert.AreEqual(filteredPass["machineID"], long.MaxValue);
      Assert.AreEqual(filteredPass["machineSpeed"], ushort.MaxValue);
      Assert.AreEqual(filteredPass["materialTemperature"], ushort.MaxValue);
      Assert.AreEqual(filteredPass["mDP"], short.MaxValue);
      Assert.AreEqual(filteredPass["radioLatency"], byte.MaxValue);
      Assert.AreEqual(filteredPass["time"], DateTime.MinValue);
      Assert.AreEqual(filteredPass["rMV"], short.MaxValue);

      var eventsValue = filteredPassData[0]["eventsValue"];

      Assert.IsNotNull(eventsValue != null);

      Assert.AreEqual(eventsValue["eventMachineRMVThreshold"], short.MaxValue);
      Assert.AreEqual(eventsValue["layerID"], ushort.MaxValue);
      Assert.AreEqual(eventsValue["mapReset_PriorDate"], DateTime.MinValue);

      var targetsValue = filteredPassData[0]["targetsValue"];

      Assert.IsNotNull(targetsValue != null);

      Assert.IsTrue(Math.Abs((float)targetsValue["targetThickness"] - float.MaxValue) < TOLERANCE);
      Assert.AreEqual(targetsValue["targetCCV"], short.MaxValue);
      Assert.AreEqual(targetsValue["targetPassCount"], ushort.MaxValue);
      Assert.AreEqual(targetsValue["tempWarningLevelMin"], ushort.MaxValue);
      Assert.AreEqual(targetsValue["tempWarningLevelMax"], ushort.MaxValue);
      Assert.AreEqual(targetsValue["targetMDP"], short.MaxValue);
    }
  }
}
