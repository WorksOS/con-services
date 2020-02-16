using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;

namespace UnitTests
{
  // TODO: Refactor setup for these tests to reduce the amount of code to set up these cases.
  [TestClass]
  public class MTSConfigDataTest
  {
    [TestMethod]
    public void MTSConfigData_SuccessWithAllData()
    {
      MTSConfigData data = new MTSConfigData();
      data.CurrentDailyReport = new MTSConfigData.DailyReportConfig();
      data.CurrentDailyReport.DailyReportTimeUTC = new TimeSpan(5, 27, 0);
      data.CurrentDailyReport.MessageSourceID = 15;
      data.CurrentDailyReport.SentUTC = DateTime.UtcNow;
      data.CurrentDailyReport.Status = MessageStatusEnum.Pending;

      data.CurrentDigitalSwitch1 = new MTSConfigData.DigitalSwitchConfig();
      data.CurrentDigitalSwitch1.Config = InputConfig.NormallyClosed;
      data.CurrentDigitalSwitch1.DelayTime = new TimeSpan(2, 22, 0);
      data.CurrentDigitalSwitch1.Description = "TEST1";
      data.CurrentDigitalSwitch1.Field = FieldID.DigitalInput1Config;
      data.CurrentDigitalSwitch1.MessageSourceID = 22;
      data.CurrentDigitalSwitch1.MonitoringCondition = DigitalInputMonitoringConditions.Always;
      data.CurrentDigitalSwitch1.SentUTC = DateTime.UtcNow;
      data.CurrentDigitalSwitch1.Status = MessageStatusEnum.Acknowledged;

      data.CurrentDigitalSwitch2 = new MTSConfigData.DigitalSwitchConfig();
      data.CurrentDigitalSwitch2.Config = InputConfig.NormallyClosed;
      data.CurrentDigitalSwitch2.DelayTime = new TimeSpan(2, 15, 0);
      data.CurrentDigitalSwitch2.Description = "TEST2";
      data.CurrentDigitalSwitch2.Field = FieldID.DigitalInput2Config;
      data.CurrentDigitalSwitch2.MessageSourceID = 22;
      data.CurrentDigitalSwitch2.MonitoringCondition = DigitalInputMonitoringConditions.KeyOffEngineOff;
      data.CurrentDigitalSwitch2.SentUTC = DateTime.UtcNow;
      data.CurrentDigitalSwitch2.Status = MessageStatusEnum.Pending;

      data.CurrentDigitalSwitch3 = new MTSConfigData.DigitalSwitchConfig();
      data.CurrentDigitalSwitch3.Config = InputConfig.NormallyClosed;
      data.CurrentDigitalSwitch3.DelayTime = new TimeSpan(3, 22, 0);
      data.CurrentDigitalSwitch3.Description = "TEST3";
      data.CurrentDigitalSwitch3.Field = FieldID.DigitalInput3Config;
      data.CurrentDigitalSwitch3.MessageSourceID = 22;
      data.CurrentDigitalSwitch3.MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOff;
      data.CurrentDigitalSwitch3.SentUTC = DateTime.UtcNow;
      data.CurrentDigitalSwitch3.Status = MessageStatusEnum.Unknown;

      data.CurrentDigitalSwitch4 = new MTSConfigData.DigitalSwitchConfig();
      data.CurrentDigitalSwitch4.Config = InputConfig.NormallyClosed;
      data.CurrentDigitalSwitch4.DelayTime = new TimeSpan(4, 33, 0);
      data.CurrentDigitalSwitch4.Description = "TEST4";
      data.CurrentDigitalSwitch4.Field = FieldID.DigitalInput4Config;
      data.CurrentDigitalSwitch4.MessageSourceID = 27;
      data.CurrentDigitalSwitch4.MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn;
      data.CurrentDigitalSwitch4.SentUTC = DateTime.UtcNow;
      data.CurrentDigitalSwitch4.Status = MessageStatusEnum.Sent;

      data.CurrentIO = new MTSConfigData.DiscreteInputConfig();
      data.CurrentIO.IO1Enabled = true;
      data.CurrentIO.IO1HysteresisHalfSeconds = 15;
      data.CurrentIO.IO1IgnRequired = false;
      data.CurrentIO.IO1PolarityIsHigh = true;
      data.CurrentIO.IO2Enabled = false;
      data.CurrentIO.IO2HysteresisHalfSeconds = 20;
      data.CurrentIO.IO2IgnRequired = true;
      data.CurrentIO.IO2PolarityIsHigh = false;
      data.CurrentIO.IO3Enabled = false;
      data.CurrentIO.IO3HysteresisHalfSeconds = 20;
      data.CurrentIO.IO3IgnRequired = true;
      data.CurrentIO.IO3PolarityIsHigh = false;
      data.CurrentIO.MessageSourceID = 33;
      data.CurrentIO.SentUTC = DateTime.UtcNow;
      data.CurrentIO.Status = MessageStatusEnum.Acknowledged;

      data.CurrentMaintMode = new MTSConfigData.MaintenanceModeConfig();
      data.CurrentMaintMode.IsEnabled = true;
      data.CurrentMaintMode.Duration = new TimeSpan(1, 35, 0);
      data.CurrentMaintMode.MessageSourceID = 25;
      data.CurrentMaintMode.SentUTC = DateTime.UtcNow;
      data.CurrentMaintMode.Status = MessageStatusEnum.Pending;

      data.CurrentMoving = new MTSConfigData.MovingConfig();
      data.CurrentMoving.MessageSourceID = 33;
      data.CurrentMoving.RadiusInFeet = 15;
      data.CurrentMoving.SentUTC = DateTime.UtcNow;
      data.CurrentMoving.Status = MessageStatusEnum.Sent;

      data.CurrentRTMileage = new MTSConfigData.MileageRuntimeConfig();
      data.CurrentRTMileage.MessageSourceID = 44;
      data.CurrentRTMileage.Mileage = 37;
      data.CurrentRTMileage.RuntimeHours = 8;
      data.CurrentRTMileage.SentUTC = DateTime.UtcNow;
      data.CurrentRTMileage.Status = MessageStatusEnum.Unknown;

      data.CurrentSpeeding = new MTSConfigData.SpeedingConfig();
      data.CurrentSpeeding.Duration = new TimeSpan(5, 13, 0);
      data.CurrentSpeeding.IsEnabled = true;
      data.CurrentSpeeding.MessageSourceID = 55;
      data.CurrentSpeeding.SentUTC = DateTime.UtcNow;
      data.CurrentSpeeding.Status = MessageStatusEnum.Acknowledged;
      data.CurrentSpeeding.ThresholdMPH = 15;

      data.CurrentStopped = new MTSConfigData.StoppedConfig();
      data.CurrentStopped.Duration = new TimeSpan(5, 13, 0);
      data.CurrentStopped.IsEnabled = true;
      data.CurrentStopped.MessageSourceID = 55;
      data.CurrentStopped.SentUTC = DateTime.UtcNow;
      data.CurrentStopped.Status = MessageStatusEnum.Pending;
      data.CurrentStopped.ThresholdMPH = 15;

      data.CurrentSMHSource = new MTSConfigData.SMHSourceConfig();
      data.CurrentSMHSource.PrimaryDataSource = 1;
      data.CurrentSMHSource.MessageSourceID = 55;
      data.CurrentSMHSource.SentUTC = DateTime.UtcNow;
      data.CurrentSMHSource.Status = MessageStatusEnum.Acknowledged;


      data.PendingDailyReport = new MTSConfigData.DailyReportConfig();
      data.PendingDailyReport.DailyReportTimeUTC = new TimeSpan(5, 27, 0);
      data.PendingDailyReport.MessageSourceID = 15;
      data.PendingDailyReport.SentUTC = DateTime.UtcNow;
      data.PendingDailyReport.Status = MessageStatusEnum.Pending;

      data.PendingDigitalSwitch1 = new MTSConfigData.DigitalSwitchConfig();
      data.PendingDigitalSwitch1.Config = InputConfig.NormallyClosed;
      data.PendingDigitalSwitch1.DelayTime = new TimeSpan(2, 22, 0);
      data.PendingDigitalSwitch1.Description = "TEST1";
      data.PendingDigitalSwitch1.Field = FieldID.DigitalInput1Config;
      data.PendingDigitalSwitch1.MessageSourceID = 22;
      data.PendingDigitalSwitch1.MonitoringCondition = DigitalInputMonitoringConditions.Always;
      data.PendingDigitalSwitch1.SentUTC = DateTime.UtcNow;
      data.PendingDigitalSwitch1.Status = MessageStatusEnum.Acknowledged;

      data.PendingDigitalSwitch2 = new MTSConfigData.DigitalSwitchConfig();
      data.PendingDigitalSwitch2.Config = InputConfig.NormallyClosed;
      data.PendingDigitalSwitch2.DelayTime = new TimeSpan(2, 15, 0);
      data.PendingDigitalSwitch2.Description = "TEST2";
      data.PendingDigitalSwitch2.Field = FieldID.DigitalInput2Config;
      data.PendingDigitalSwitch2.MessageSourceID = 22;
      data.PendingDigitalSwitch2.MonitoringCondition = DigitalInputMonitoringConditions.KeyOffEngineOff;
      data.PendingDigitalSwitch2.SentUTC = DateTime.UtcNow;
      data.PendingDigitalSwitch2.Status = MessageStatusEnum.Pending;

      data.PendingDigitalSwitch3 = new MTSConfigData.DigitalSwitchConfig();
      data.PendingDigitalSwitch3.Config = InputConfig.NormallyClosed;
      data.PendingDigitalSwitch3.DelayTime = new TimeSpan(3, 22, 0);
      data.PendingDigitalSwitch3.Description = "TEST3";
      data.PendingDigitalSwitch3.Field = FieldID.DigitalInput3Config;
      data.PendingDigitalSwitch3.MessageSourceID = 22;
      data.PendingDigitalSwitch3.MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOff;
      data.PendingDigitalSwitch3.SentUTC = DateTime.UtcNow;
      data.PendingDigitalSwitch3.Status = MessageStatusEnum.Unknown;

      data.PendingDigitalSwitch4 = new MTSConfigData.DigitalSwitchConfig();
      data.PendingDigitalSwitch4.Config = InputConfig.NormallyClosed;
      data.PendingDigitalSwitch4.DelayTime = new TimeSpan(4, 33, 0);
      data.PendingDigitalSwitch4.Description = "TEST4";
      data.PendingDigitalSwitch4.Field = FieldID.DigitalInput4Config;
      data.PendingDigitalSwitch4.MessageSourceID = 27;
      data.PendingDigitalSwitch4.MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn;
      data.PendingDigitalSwitch4.SentUTC = DateTime.UtcNow;
      data.PendingDigitalSwitch4.Status = MessageStatusEnum.Sent;

      data.PendingIO = new MTSConfigData.DiscreteInputConfig();
      data.PendingIO.IO1Enabled = true;
      data.PendingIO.IO1HysteresisHalfSeconds = 15;
      data.PendingIO.IO1IgnRequired = false;
      data.PendingIO.IO1PolarityIsHigh = true;
      data.PendingIO.IO2Enabled = false;
      data.PendingIO.IO2HysteresisHalfSeconds = 20;
      data.PendingIO.IO2IgnRequired = true;
      data.PendingIO.IO2PolarityIsHigh = false;
      data.PendingIO.IO3Enabled = false;
      data.PendingIO.IO3HysteresisHalfSeconds = 20;
      data.PendingIO.IO3IgnRequired = true;
      data.PendingIO.IO3PolarityIsHigh = false;
      data.PendingIO.MessageSourceID = 33;
      data.PendingIO.SentUTC = DateTime.UtcNow;
      data.PendingIO.Status = MessageStatusEnum.Acknowledged;

      data.PendingMaintMode = new MTSConfigData.MaintenanceModeConfig();
      data.PendingMaintMode.IsEnabled = true;
      data.PendingMaintMode.Duration = new TimeSpan(1, 35, 0);
      data.PendingMaintMode.MessageSourceID = 25;
      data.PendingMaintMode.SentUTC = DateTime.UtcNow;
      data.PendingMaintMode.Status = MessageStatusEnum.Pending;

      data.PendingMoving = new MTSConfigData.MovingConfig();
      data.PendingMoving.MessageSourceID = 33;
      data.PendingMoving.RadiusInFeet = 15;
      data.PendingMoving.SentUTC = DateTime.UtcNow;
      data.PendingMoving.Status = MessageStatusEnum.Sent;

      data.PendingRTMileage = new MTSConfigData.MileageRuntimeConfig();
      data.PendingRTMileage.MessageSourceID = 44;
      data.PendingRTMileage.Mileage = 37;
      data.PendingRTMileage.RuntimeHours = 8;
      data.PendingRTMileage.SentUTC = DateTime.UtcNow;
      data.PendingRTMileage.Status = MessageStatusEnum.Unknown;

      data.PendingPasscode = new MTSConfigData.PasscodeConfig();
      data.PendingPasscode.MessageSourceID = 121;
      data.PendingPasscode.Passcode = "589191-TRCE1-00000-94933D16";
      data.PendingPasscode.SentUTC = DateTime.UtcNow;
      data.PendingPasscode.Status = MessageStatusEnum.Acknowledged;

      data.PendingSpeeding = new MTSConfigData.SpeedingConfig();
      data.PendingSpeeding.Duration = new TimeSpan(5, 13, 0);
      data.PendingSpeeding.IsEnabled = true;
      data.PendingSpeeding.MessageSourceID = 55;
      data.PendingSpeeding.SentUTC = DateTime.UtcNow;
      data.PendingSpeeding.Status = MessageStatusEnum.Acknowledged;
      data.PendingSpeeding.ThresholdMPH = 15;

      data.PendingStopped = new MTSConfigData.StoppedConfig();
      data.PendingStopped.Duration = new TimeSpan(5, 13, 0);
      data.PendingStopped.IsEnabled = true;
      data.PendingStopped.MessageSourceID = 55;
      data.PendingStopped.SentUTC = DateTime.UtcNow;
      data.PendingStopped.Status = MessageStatusEnum.Pending;
      data.PendingStopped.ThresholdMPH = 15;

      data.CurrentSMHSource = new MTSConfigData.SMHSourceConfig();
      data.CurrentSMHSource.PrimaryDataSource = 0;
      data.CurrentSMHSource.MessageSourceID = 55;
      data.CurrentSMHSource.SentUTC = DateTime.UtcNow;
      data.CurrentSMHSource.Status = MessageStatusEnum.Pending;

      XElement dataElement = data.ToXElement();
      MTSConfigData data2 = new MTSConfigData(dataElement);
      Assert.AreEqual(dataElement.ToString(), data2.ToXElement().ToString(), "XML Does Not Equal");

    }

    [TestMethod]
    [Ignore]
    public void LoadCrosscheckConfigByType_Success()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureWhenBitPacketIntervalSecondsLessThanOneAndTotalSecondsGreaterThanMaxValue()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureWhenLowPowerIntervalSecondsLessThanOneAndTotalSecondsGreaterThanMaxValue()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureWhenReportingIntervalSecondsLessThanOneAndTotalSecondsGreaterThanMaxValue()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureWhenSamplingIntervalSecondsLessThanOneAndTotalSecondsGreaterThanMaxValue()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureWhenDeviceNull()
    {

    }


    [TestMethod]
    [Ignore]
    public void UpdateSamplingIntervals_SuccessWhenDeviceExists()
    {

    }

    [TestMethod]
    [Ignore]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UpdateSamplingIntervals_FailureSaveChangeException()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetPrimaryIPAddress_SuccessWhenMessageExists()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigMsg_SuccessWhenMessageExists()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigMsg_SuccessWhenBaseMessageExists()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetNetworkInterfaceConfig_SuccessWhenMessageExists()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForConfigureSensors()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForDevicePortConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForGeneralDeviceConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForHomeSitePositionReportingConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForIgnitionEventsEnabled()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForMovingConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForPrimaryIPAddressConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForSpeedingThreshold()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForStoppedThreshold()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForZoneLogic()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForDailyReportConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForNetworkInterfaceConfig()
    {

    }

    [TestMethod]
    [Ignore]
    public void GetDeviceConfigType_SuccessForDefault()
    {

    }

    [TestMethod]
    public void MTSConfigData_SuccessWithOnePendingOneCurrent()
    {
      MTSConfigData data = new MTSConfigData();

      data.CurrentMaintMode = new MTSConfigData.MaintenanceModeConfig();
      data.CurrentMaintMode.IsEnabled = true;
      data.CurrentMaintMode.Duration = new TimeSpan(1, 35, 0);
      data.CurrentMaintMode.MessageSourceID = 25;
      data.CurrentMaintMode.SentUTC = DateTime.UtcNow;
      data.CurrentMaintMode.Status = MessageStatusEnum.Pending;


      data.PendingDailyReport = new MTSConfigData.DailyReportConfig();
      data.PendingDailyReport.DailyReportTimeUTC = new TimeSpan(5, 27, 0);
      data.PendingDailyReport.MessageSourceID = 15;
      data.PendingDailyReport.SentUTC = DateTime.UtcNow;
      data.PendingDailyReport.Status = MessageStatusEnum.Pending;

      XElement dataElement = data.ToXElement();
      MTSConfigData data2 = new MTSConfigData(dataElement);
      Assert.AreEqual(dataElement.ToString(), data2.ToXElement().ToString(), "XML Does Not Equal");

    }

    [TestMethod]
    public void MTSConfigDataUpdate_Success()
    {
      MTSConfigData data = new MTSConfigData();
      data.CurrentDailyReport = new MTSConfigData.DailyReportConfig();
      data.CurrentDailyReport.MessageSourceID = 15;
      data.CurrentDailyReport.DailyReportTimeUTC = new TimeSpan(2, 3, 4, 5, 6);
      data.CurrentDailyReport.SentUTC = DateTime.UtcNow;
      data.CurrentDailyReport.Status = MessageStatusEnum.Acknowledged;

      data.PendingDailyReport = new MTSConfigData.DailyReportConfig();
      data.PendingDailyReport.MessageSourceID = 18;
      data.PendingDailyReport.DailyReportTimeUTC = new TimeSpan(2, 3, 4, 5, 6);
      data.PendingDailyReport.SentUTC = DateTime.UtcNow;
      data.PendingDailyReport.Status = MessageStatusEnum.Sent;

      MTSConfigData.DailyReportConfig newConfig = new MTSConfigData.DailyReportConfig();
      newConfig.MessageSourceID = 110;
      newConfig.DailyReportTimeUTC = new TimeSpan(2, 3, 4, 5, 6);
      newConfig.SentUTC = DateTime.UtcNow;
      newConfig.Status = MessageStatusEnum.Acknowledged;

      data.Update(newConfig);

      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentDailyReport.MessageSourceID, "current should have the new config");
      Assert.IsNotNull(data.PendingDailyReport, "pending should not be null");

      newConfig.MessageSourceID = 18;
      data.Update(newConfig);
      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentDailyReport.MessageSourceID, "current should have the new config");
      Assert.IsNull(data.PendingDailyReport, "pending should be null");
    }

    [TestMethod]
    public void MileageRuntimeConfig_Success()
    {
      MTSConfigData.MileageRuntimeConfig config = new MTSConfigData.MileageRuntimeConfig();
      config.MessageSourceID = 15;
      config.Mileage = 20;
      config.RuntimeHours = 25;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.MileageRuntimeConfig configAfterXElement = new MTSConfigData.MileageRuntimeConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.Mileage, configAfterXElement.Mileage, "Incorrect Mileage");
      Assert.AreEqual(config.RuntimeHours, configAfterXElement.RuntimeHours, "Incorrect RunTimeHours");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
    }

    [TestMethod]
    public void PasscodeConfig_Success()
    {
      MTSConfigData.PasscodeConfig config = new MTSConfigData.PasscodeConfig();
      config.MessageSourceID = 15;
      config.Passcode = "589191-TRG11-00000-08FEC7D4";
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Pending;

      MTSConfigData.PasscodeConfig configAfterXElement = new MTSConfigData.PasscodeConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.Passcode, configAfterXElement.Passcode, "Incorrect Passcode");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
    }

    [TestMethod]
    public void DailyReportConfig_Success()
    {
      MTSConfigData.DailyReportConfig config = new MTSConfigData.DailyReportConfig();
      config.MessageSourceID = 15;
      config.DailyReportTimeUTC = new TimeSpan(2, 3, 4, 5, 6);
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.DailyReportConfig configAfterXElement = new MTSConfigData.DailyReportConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.DailyReportTimeUTC, configAfterXElement.DailyReportTimeUTC, "Incorrect DailyReportTime");
    }

    [TestMethod]
    public void SpeedingConfig_Success()
    {
      MTSConfigData.SpeedingConfig config = new MTSConfigData.SpeedingConfig();
      config.MessageSourceID = 15;
      config.Duration = new TimeSpan(2, 3, 4, 5, 6);
      config.IsEnabled = true;
      config.ThresholdMPH = 25;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.SpeedingConfig configAfterXElement = new MTSConfigData.SpeedingConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.Duration, configAfterXElement.Duration, "Incorrect Duration");
      Assert.AreEqual(config.IsEnabled, configAfterXElement.IsEnabled, "Incorrect IsEnabled");
      Assert.AreEqual(config.ThresholdMPH, configAfterXElement.ThresholdMPH, "Incorrect ThresholdMPH");
    }

    [TestMethod]
    public void MovingConfig_Success()
    {
      MTSConfigData.MovingConfig config = new MTSConfigData.MovingConfig();
      config.MessageSourceID = 15;
      config.RadiusInFeet = 15;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.MovingConfig configAfterXElement = new MTSConfigData.MovingConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.RadiusInFeet, configAfterXElement.RadiusInFeet, "Incorrect RadiusInFeet");
    }

    [TestMethod]
    public void StoppedConfig_Success()
    {
      MTSConfigData.StoppedConfig config = new MTSConfigData.StoppedConfig();
      config.MessageSourceID = 15;
      config.Duration = new TimeSpan(2, 3, 4, 5, 6);
      config.IsEnabled = true;
      config.ThresholdMPH = 25;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.StoppedConfig configAfterXElement = new MTSConfigData.StoppedConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.Duration, configAfterXElement.Duration, "Incorrect Duration");
      Assert.AreEqual(config.IsEnabled, configAfterXElement.IsEnabled, "Incorrect IsEnabled");
      Assert.AreEqual(config.ThresholdMPH, configAfterXElement.ThresholdMPH, "Incorrect ThresholdMPH");
    }

    [TestMethod]
    public void MaintenanceModeConfig_Success()
    {
      MTSConfigData.MaintenanceModeConfig config = new MTSConfigData.MaintenanceModeConfig();
      config.MessageSourceID = 15;
      config.Duration = new TimeSpan(2, 3, 4, 5, 6);
      config.IsEnabled = true;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.MaintenanceModeConfig configAfterXElement = new MTSConfigData.MaintenanceModeConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.Duration, configAfterXElement.Duration, "Incorrect Duration");
      Assert.AreEqual(config.IsEnabled, configAfterXElement.IsEnabled, "Incorrect IsEnabled");
    }

    [TestMethod]
    public void DiscreteInputConfig_Success()
    {
      MTSConfigData.DiscreteInputConfig config = new MTSConfigData.DiscreteInputConfig();
      config.MessageSourceID = 15;
      config.IO1Enabled = true;
      config.IO1HysteresisHalfSeconds = 2;
      config.IO1IgnRequired = true;
      config.IO1PolarityIsHigh = true;
      config.IO2Enabled = false;
      config.IO2HysteresisHalfSeconds = 4;
      config.IO2IgnRequired = false;
      config.IO2PolarityIsHigh = false;
      config.IO3Enabled = true;
      config.IO3HysteresisHalfSeconds = 6;
      config.IO3IgnRequired = false;
      config.IO3PolarityIsHigh = true;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.DiscreteInputConfig configAfterXElement = new MTSConfigData.DiscreteInputConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.IO1Enabled, configAfterXElement.IO1Enabled, "Incorrect IO1Enabled");
      Assert.AreEqual(config.IO2Enabled, configAfterXElement.IO2Enabled, "Incorrect IO1Enabled");
      Assert.AreEqual(config.IO3Enabled, configAfterXElement.IO3Enabled, "Incorrect IO3Enabled");
      Assert.AreEqual(config.IO1HysteresisHalfSeconds, configAfterXElement.IO1HysteresisHalfSeconds, "Incorrect IO1HysteresisHalfSeconds");
      Assert.AreEqual(config.IO2HysteresisHalfSeconds, configAfterXElement.IO2HysteresisHalfSeconds, "Incorrect IO2HysteresisHalfSeconds");
      Assert.AreEqual(config.IO3HysteresisHalfSeconds, configAfterXElement.IO3HysteresisHalfSeconds, "Incorrect IO3HysteresisHalfSeconds");
      Assert.AreEqual(config.IO1IgnRequired, configAfterXElement.IO1IgnRequired, "Incorrect IO1IgnRequired");
      Assert.AreEqual(config.IO2IgnRequired, configAfterXElement.IO2IgnRequired, "Incorrect IO2IgnRequired");
      Assert.AreEqual(config.IO3IgnRequired, configAfterXElement.IO3IgnRequired, "Incorrect IO3IgnRequired");
      Assert.AreEqual(config.IO1PolarityIsHigh, configAfterXElement.IO1PolarityIsHigh, "Incorrect IO1PolarityIsHigh");
      Assert.AreEqual(config.IO2PolarityIsHigh, configAfterXElement.IO2PolarityIsHigh, "Incorrect IO2PolarityIsHigh");
      Assert.AreEqual(config.IO3PolarityIsHigh, configAfterXElement.IO3PolarityIsHigh, "Incorrect IO3PolarityIsHigh");
    }

    [TestMethod]
    public void DigitalSwitchConfig_Success()
    {
      MTSConfigData.DigitalSwitchConfig config = new MTSConfigData.DigitalSwitchConfig();
      config.MessageSourceID = 15;
      config.Config = InputConfig.NormallyClosed;
      config.DelayTime = new TimeSpan(1, 2, 3, 4);
      config.Description = "TEST";
      config.Field = FieldID.DigitalInput1Config;
      config.MonitoringCondition = DigitalInputMonitoringConditions.Always;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.DigitalSwitchConfig configAfterXElement = new MTSConfigData.DigitalSwitchConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.Config, configAfterXElement.Config, "Incorrect Config");
      Assert.AreEqual(config.DelayTime, configAfterXElement.DelayTime, "Incorrect DelayTime");
      Assert.AreEqual(config.Description, configAfterXElement.Description, "Incorrect Description");
      Assert.AreEqual(config.Field, configAfterXElement.Field, "Incorrect Field");
      Assert.AreEqual(config.MonitoringCondition, configAfterXElement.MonitoringCondition, "Incorrect MonitoringCondition");
    }

    [TestMethod]
    public void SMHDataSource_Success()
    {
      MTSConfigData.SMHSourceConfig config = new MTSConfigData.SMHSourceConfig();
      config.MessageSourceID = 15;
      config.PrimaryDataSource = 1;
      config.SentUTC = DateTime.UtcNow;
      config.Status = MessageStatusEnum.Acknowledged;

      MTSConfigData.SMHSourceConfig configAfterXElement = new MTSConfigData.SMHSourceConfig(config.ToXElement());
      Assert.AreEqual(config.MessageSourceID, configAfterXElement.MessageSourceID, "Incorrect MessageSourceID");
      Assert.AreEqual(config.SentUTC, configAfterXElement.SentUTC, "Incorrect SentUTC");
      Assert.AreEqual(config.Status, configAfterXElement.Status, "Incorrect Status");
      Assert.AreEqual(config.PrimaryDataSource, configAfterXElement.PrimaryDataSource, "Incorrect Config");
    }

    [TestMethod]
    public void SetSMHDataSource_Vehicle_Success()
    {
      MTSConfigData data = new MTSConfigData();
      data.CurrentSMHSource = new MTSConfigData.SMHSourceConfig();
      data.CurrentSMHSource.MessageSourceID = 15;
      data.CurrentSMHSource.PrimaryDataSource = 1;
      data.CurrentSMHSource.SentUTC = DateTime.UtcNow;
      data.CurrentSMHSource.Status = MessageStatusEnum.Acknowledged;

      data.PendingSMHSource = new MTSConfigData.SMHSourceConfig();
      data.PendingSMHSource.MessageSourceID = 18;
      data.PendingSMHSource.PrimaryDataSource = 0;
      data.PendingSMHSource.SentUTC = DateTime.UtcNow;
      data.PendingSMHSource.Status = MessageStatusEnum.Sent;

      MTSConfigData.SMHSourceConfig newConfig = new MTSConfigData.SMHSourceConfig();
      newConfig.MessageSourceID = 110;
      newConfig.PrimaryDataSource = 1;
      newConfig.SentUTC = DateTime.UtcNow;
      newConfig.Status = MessageStatusEnum.Acknowledged;

      data.Update(newConfig);

      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentSMHSource.MessageSourceID, "current should have the new config");
      Assert.IsNotNull(data.PendingSMHSource, "pending should not be null");

      newConfig.MessageSourceID = 18;
      data.Update(newConfig);
      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentSMHSource.MessageSourceID, "current should have the new config");
      Assert.AreEqual(newConfig.PrimaryDataSource, data.CurrentSMHSource.PrimaryDataSource, "current should have the new config");
      Assert.IsNull(data.PendingSMHSource, "pending should be null");
    }

    [TestMethod]
    public void SetSMHDataSource_RTerminal_Success()
    {
      MTSConfigData data = new MTSConfigData();
      data.CurrentSMHSource = new MTSConfigData.SMHSourceConfig();
      data.CurrentSMHSource.MessageSourceID = 15;
      data.CurrentSMHSource.PrimaryDataSource = 0;
      data.CurrentSMHSource.SentUTC = DateTime.UtcNow;
      data.CurrentSMHSource.Status = MessageStatusEnum.Acknowledged;

      data.PendingSMHSource = new MTSConfigData.SMHSourceConfig();
      data.PendingSMHSource.MessageSourceID = 18;
      data.PendingSMHSource.PrimaryDataSource = 1;
      data.PendingSMHSource.SentUTC = DateTime.UtcNow;
      data.PendingSMHSource.Status = MessageStatusEnum.Sent;

      MTSConfigData.SMHSourceConfig newConfig = new MTSConfigData.SMHSourceConfig();
      newConfig.MessageSourceID = 110;
      newConfig.PrimaryDataSource = 0;
      newConfig.SentUTC = DateTime.UtcNow;
      newConfig.Status = MessageStatusEnum.Acknowledged;

      data.Update(newConfig);

      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentSMHSource.MessageSourceID, "current should have the new config");
      Assert.IsNotNull(data.PendingSMHSource, "pending should not be null");

      newConfig.MessageSourceID = 18;
      data.Update(newConfig);
      Assert.AreEqual(newConfig.MessageSourceID, data.CurrentSMHSource.MessageSourceID, "current should have the new config");
      Assert.AreEqual(newConfig.PrimaryDataSource, data.CurrentSMHSource.PrimaryDataSource, "current should have the new config");
      Assert.IsNull(data.PendingSMHSource, "pending should be null");
    }

    [TestMethod]
    public void TestGetFormattedStartMode()
    {
      MachineStartStatus? result = MachineStartStatus.NotConfigured;
      MTSUpdateDeviceConfig_Accessor.GetFormattedStartMode(MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureDisabled,ref result);
      Assert.AreEqual(MachineStartStatus.NormalOperation, result, "Expected Normal Operation");

      MTSUpdateDeviceConfig_Accessor.GetFormattedStartMode(MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled, ref result);
      Assert.AreEqual(MachineStartStatus.NormalOperation, result, "Expected Normal Operation");

      MTSUpdateDeviceConfig_Accessor.GetFormattedStartMode(MachineSecurityModeSetting.Disabled, ref result);
      Assert.AreEqual(MachineStartStatus.Disabled, result, "Expected Disabled Mode");

      MTSUpdateDeviceConfig_Accessor.GetFormattedStartMode(MachineSecurityModeSetting.MachineInDisableModebutsecuritytamperedorbypass, ref result);
      Assert.AreEqual(MachineStartStatus.Disabled, result, "Expected Disabled Mode");

      MTSUpdateDeviceConfig_Accessor.GetFormattedStartMode(MachineSecurityModeSetting.MachineInDisableModebutPowercut, ref result);
      Assert.AreEqual(MachineStartStatus.Disabled, result, "Expected Disabled Mode");
    }

    [TestMethod]
    public void TestUpdateForTamperSecurityStatusInformation_NoPendingConfigChanges_ExpectCurrentStartModeChanged()
    {
      //Arrange
      string dateFormat = "MM-dd-yyyy hh:mm:ss";
      var machineSecurityStatusConfig3 = new MTSConfigData.TamperSecurityAdministrationInformationConfig
      {
        machineStartStatus = MachineStartStatus.NormalOperation,
        machineStartStatusField = FieldID.MachineStartMode,
        MessageSourceID = 0,
        SentUTC = DateTime.ParseExact("05-28-2014 09:50:56", dateFormat, CultureInfo.InvariantCulture),
        machineStartModeConfigurationSource =  MachineStartModeConfigurationSource.OffBoardOfficeSystemVL
      };
      ;
      machineSecurityStatusConfig3.Status = MessageStatusEnum.Acknowledged;
      machineSecurityStatusConfig3.packetID = 70;
      machineSecurityStatusConfig3.machineStartStatusTrigger = MachineStartStatusTrigger.OTACommand;

      var currentConfig = new MTSConfigData.TamperSecurityAdministrationInformationConfig
      {
        machineStartStatus = MachineStartStatus.Disabled,
        machineStartStatusField = FieldID.MachineStartMode
      };

      var mtsConfig = new MTSConfigData {currentMachineSecuritySystemInformationConfig = currentConfig};

      //Act
      mtsConfig.Update(machineSecurityStatusConfig3);

      //Assert
      MTSConfigData.TamperSecurityAdministrationInformationConfig current = mtsConfig.currentMachineSecuritySystemInformationConfig;

      Assert.IsNull(mtsConfig.pendingMachineSecuritySystemInformationConfig);
      Assert.IsTrue(current.machineStartStatus.Equals(MachineStartStatus.NormalOperation));
      Assert.IsTrue(current.machineStartStatusTrigger.Equals(MachineStartStatusTrigger.OTACommand));
      Assert.IsTrue(current.machineStartModeConfigurationSource.Equals(MachineStartModeConfigurationSource.OffBoardOfficeSystemVL));
    }

    [TestMethod]
    public void TestUpdateForTamperSecurityStatusInformation_PendingConfigChangesPresent_ExpectCurrentStartModeChanged()
    {
      //Arrange
      const string dateFormat = "MM-dd-yyyy hh:mm:ss";
      var machineSecurityStatusConfig3 = new MTSConfigData.TamperSecurityAdministrationInformationConfig
      {
        machineStartStatus = MachineStartStatus.Disabled,
        machineStartStatusField = FieldID.MachineStartMode,
        MessageSourceID = 0,
        SentUTC = DateTime.ParseExact("05-28-2014 09:50:56", dateFormat, CultureInfo.InvariantCulture),
        Status =  MessageStatusEnum.Acknowledged,
        packetID = 70,
        machineStartStatusTrigger = MachineStartStatusTrigger.OTACommand,
        machineStartModeConfigurationSource = MachineStartModeConfigurationSource.NeverConfigured
      };
      var pendingConfig = new MTSConfigData.TamperSecurityAdministrationInformationConfig
      {
        machineStartStatus = MachineStartStatus.Disabled,
        machineStartStatusField = FieldID.MachineStartMode,
        MessageSourceID = 12345,
        SentUTC = DateTime.ParseExact("05-28-2014 09:49:50", dateFormat, CultureInfo.InvariantCulture),
        Status =  MessageStatusEnum.Pending,
        machineStartStatusTrigger = MachineStartStatusTrigger.TamperUninstalled,
        packetID = 70,
        machineStartModeConfigurationSource = MachineStartModeConfigurationSource.OffBoardOfficeSystemVL
      };
     
      var currentConfig = new MTSConfigData.TamperSecurityAdministrationInformationConfig
      {
        machineStartStatus = MachineStartStatus.NormalOperation,
        machineStartStatusField = FieldID.MachineStartMode
      };

      var mtsConfig = new MTSConfigData { currentMachineSecuritySystemInformationConfig = currentConfig, pendingMachineSecuritySystemInformationConfig = pendingConfig};

      //Act
      mtsConfig.Update(machineSecurityStatusConfig3);

      //Assert
      MTSConfigData.TamperSecurityAdministrationInformationConfig current = mtsConfig.currentMachineSecuritySystemInformationConfig;

      Assert.IsNull(mtsConfig.pendingMachineSecuritySystemInformationConfig.packetID);
      Assert.IsTrue(current.machineStartStatus.Equals(MachineStartStatus.Disabled));
      Assert.IsTrue(current.machineStartStatusTrigger.Equals(MachineStartStatusTrigger.OTACommand));
      Assert.IsTrue(current.machineStartModeConfigurationSource.Equals(MachineStartModeConfigurationSource.OffBoardOfficeSystemVL));
    }


  }
}
