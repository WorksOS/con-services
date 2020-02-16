using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  /// <summary>
  ///This is a test class for PLConfigDataTest and is intended
  ///to contain all PLConfigDataTest Unit Tests
  ///</summary>
  [TestClass]
  public class PLConfigDataTest
  {
    private const int Seed = 123456; // make unit-tests run the same every time

    [TestMethod]
    public void CanGenerateGeneralRegistry()
    {
      var config = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);

      Assert.IsNotNull(config.GlobalGramEnable, "should be filled");
      Assert.IsNotNull(config.ReportSchedule, "should be filled");
      Assert.IsFalse(string.IsNullOrWhiteSpace(config.ModuleType), "should be filled");
      Assert.IsNotNull(config.ReportSchedule.Reports, "should be filled");
      Assert.IsTrue(config.ReportSchedule.Reports.Count > 0, "should be filled");
      Assert.IsNotNull(config.ReportSchedule.Reports[0], "should be filled");
      Assert.IsNotNull(config.ReportSchedule.Reports[0].ReportType, "should be filled");
    }

    [TestMethod]
    public void CanGenerateTransmissionRegistry()
    {
      var config = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);

      Assert.IsNotNull(config.EventIntervalHours, "should be filled");
      Assert.IsNotNull(config.EventIntervalHoursSentUTC, "should be filled");
      Assert.IsNotNull(config.EventReporting, "should be filled");
      Assert.IsNotNull(config.EventReporting.DiagnosticFreqCode, "should be filled");
    }

    [TestMethod]
    public void CanGenerateDigitalRegistry()
    {
      var config = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      for (var sensorNumber = 0; sensorNumber < config.Sensors.Count; sensorNumber++)
        config.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique

      Assert.IsNotNull(config.Sensors, "should be filled");
      Assert.IsTrue(config.Sensors.Count > 0, "should be filled");
      Assert.IsNotNull(config.Sensors[0].DelayTime, "should be filled");
    }

    [TestMethod]
    public void PLConfigDataAllConfigsTest()
    {
      var data = new PLConfigData
      {
        CurrentDigitalRegistry = new PLConfigData.DigitalRegistry(),
        PendingDigitalRegistry = new PLConfigData.DigitalRegistry(),
        CurrentGeneralRegistry = new PLConfigData.GeneralRegistry(),
        PendingGeneralRegistry = new PLConfigData.GeneralRegistry(),
        CurrentTransmissionRegistry = new PLConfigData.TransmissionRegistry(),
        PendingTransmissionRegistry = new PLConfigData.TransmissionRegistry()
      };

      //current sensors
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>();
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test",
        MonitorCondition = DigitalInputMonitoringConditions.Always,
        SensorConfiguration = InputConfig.NormallyClosed,
        SensorNumber = 1
      };
//      sensor1.DelayTimeSentUTC = DateTime.UtcNow; 
//      sensor1.DescriptionSentUTC = DateTime.UtcNow;
//      sensor1.MonitorConditionSentUTC = DateTime.UtcNow;
//      sensor1.SensorConfigSentUTC = DateTime.UtcNow;
      data.CurrentDigitalRegistry.Sensors.Add(sensor1);

      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test2",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOffEngineOff,
        SensorConfiguration = InputConfig.NormallyOpen,
        SensorNumber = 2
      };
      data.CurrentDigitalRegistry.Sensors.Add(sensor2);

      //pending sensors
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>();
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test3",
        MonitorCondition = DigitalInputMonitoringConditions.Always,
        SensorConfiguration = InputConfig.NormallyClosed,
        SensorNumber = 3
      };
//      sensor3.DelayTimeSentUTC = DateTime.UtcNow;
//      sensor3.DescriptionSentUTC = DateTime.UtcNow;
//      sensor3.MonitorConditionSentUTC = DateTime.UtcNow;
//      sensor3.SensorConfigSentUTC = DateTime.UtcNow;
      data.PendingDigitalRegistry.Sensors.Add(sensor3);

      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test4",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOffEngineOff,
        SensorConfiguration = InputConfig.NormallyOpen,
        SensorNumber = 4
      };
      data.PendingDigitalRegistry.Sensors.Add(sensor4);

      //general Current
      data.CurrentGeneralRegistry.BlockDataTransfer = false;
      data.CurrentGeneralRegistry.DataLinkType = "2";
      data.CurrentGeneralRegistry.GlobalGramEnable = true;
      data.CurrentGeneralRegistry.LastRegistrationDate = DateTime.UtcNow;
      data.CurrentGeneralRegistry.ModuleType = ModuleTypeEnum.PL121.ToString();
      data.CurrentGeneralRegistry.RegDealerCode = "6";
      data.CurrentGeneralRegistry.RegistrationStatus = "True";
      data.CurrentGeneralRegistry.RunTimeHoursAdj = TimeSpan.FromDays(1.234345);
      data.CurrentGeneralRegistry.StartStopEnable = false;
      data.CurrentGeneralRegistry.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule
      {
        ReportStartTime = TimeSpan.FromMinutes(2.11134),
        Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>()
      };
      var pos = new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 4, ReportType = "Position"};
//      pos.SentUTC = DateTime.UtcNow;
      data.CurrentGeneralRegistry.ReportSchedule.Reports.Add(pos);
      var smu = new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = null, ReportType = "SMU"};
//      smu.SentUTC = DateTime.UtcNow;
      data.CurrentGeneralRegistry.ReportSchedule.Reports.Add(smu);
      data.CurrentGeneralRegistry.Software = new PLConfigData.GeneralRegistry.SoftwareInfo
      {
        HardwareSerialNumber = "1234",
        HC11SoftwarePartNumber = "12345",
        ModemSoftwarePartNumber = "123456",
        SoftwareRevision = "1234567"
      };

      //general Pending
      data.PendingGeneralRegistry.BlockDataTransfer = false;
      data.PendingGeneralRegistry.DataLinkType = "2";
      data.PendingGeneralRegistry.GlobalGramEnable = true;
      data.PendingGeneralRegistry.LastRegistrationDate = DateTime.UtcNow;
      data.PendingGeneralRegistry.ModuleType = ModuleTypeEnum.PL300.ToString();
      data.PendingGeneralRegistry.RegDealerCode = "6";
      data.PendingGeneralRegistry.RegistrationStatus = "True";
      data.PendingGeneralRegistry.RunTimeHoursAdj = TimeSpan.FromDays(1.234345);
      data.PendingGeneralRegistry.StartStopEnable = false;
      data.PendingGeneralRegistry.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule
      {
        ReportStartTime = TimeSpan.FromMinutes(2.11134),
        Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>()
      };
      var pos1 = new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 3, ReportType = "Position"};
      data.PendingGeneralRegistry.ReportSchedule.Reports.Add(pos1);
      var smu1 = new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 2, ReportType = "SMU"};
      data.PendingGeneralRegistry.ReportSchedule.Reports.Add(smu1);
      data.PendingGeneralRegistry.Software = new PLConfigData.GeneralRegistry.SoftwareInfo
      {
        HardwareSerialNumber = "123",
        HC11SoftwarePartNumber = "1235",
        ModemSoftwarePartNumber = "12356",
        SoftwareRevision = "123567"
      };

      //Transmission Current
      data.CurrentTransmissionRegistry.EventIntervalHours = 2;
      data.CurrentTransmissionRegistry.NextMessageInterval = 22;
      data.CurrentTransmissionRegistry.SMUFuel = SMUFuelReporting.Fuel;
      data.CurrentTransmissionRegistry.EventReporting = new PLConfigData.TransmissionRegistry.EventReportingFrequency
      {
        DiagnosticFreqCode = EventFrequency.Immediately,
        Level1EventFreqCode = EventFrequency.Never,
        Level2EventFreqCode = EventFrequency.Next,
        Level3EventFreqCode = EventFrequency.Unknown
      };

      //Transmission Pending
      data.PendingTransmissionRegistry.EventIntervalHours = 22;
      data.PendingTransmissionRegistry.NextMessageInterval = 23;
      data.PendingTransmissionRegistry.SMUFuel = SMUFuelReporting.Off;
      data.PendingTransmissionRegistry.EventReporting = new PLConfigData.TransmissionRegistry.EventReportingFrequency
      {
        DiagnosticFreqCode = EventFrequency.Unknown,
        Level1EventFreqCode = EventFrequency.Next,
        Level2EventFreqCode = EventFrequency.Never,
        Level3EventFreqCode = EventFrequency.Immediately
      };

      var newData = new PLConfigData(data.ToXElement());
      Assert.AreEqual(data.ToXElement().ToString(), newData.ToXElement().ToString(), "Incorrect XML");
    }

    [TestMethod]
    public void TestNullXElementPLConfigData()
    {
      var newData = new PLConfigData((XElement) null);
      Assert.IsNull(newData.PendingGeneralRegistry);
    }

    [TestMethod]
    public void TestNullStringPLConfigData()
    {
      var newData = new PLConfigData((string)null);
      Assert.IsNull(newData.PendingGeneralRegistry);
    }

    [TestMethod]
    public void PLConfigDataAllConfigsTest_OldestPendingKeyDate_Success()
    {
      var data = new PLConfigData
      {
        CurrentDigitalRegistry = new PLConfigData.DigitalRegistry(),
        PendingDigitalRegistry = new PLConfigData.DigitalRegistry(),
        CurrentGeneralRegistry = new PLConfigData.GeneralRegistry(),
        PendingGeneralRegistry = new PLConfigData.GeneralRegistry(),
        CurrentTransmissionRegistry = new PLConfigData.TransmissionRegistry(),
        PendingTransmissionRegistry = new PLConfigData.TransmissionRegistry()
      };

      var oldestTimestamp = DateTime.UtcNow.AddDays(-8);
      var olderTimestamp = DateTime.UtcNow.AddDays(-1);


      //current sensors
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>();
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test",
        MonitorCondition = DigitalInputMonitoringConditions.Always,
        SensorConfiguration = InputConfig.NormallyClosed,
        SensorNumber = 1,
        DelayTimeSentUTC = olderTimestamp,
        DescriptionSentUTC = DateTime.UtcNow,
        MonitorConditionSentUTC = DateTime.UtcNow,
        SensorConfigSentUTC = DateTime.UtcNow
      };
      data.CurrentDigitalRegistry.Sensors.Add(sensor1);

      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test2",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOffEngineOff,
        SensorConfiguration = InputConfig.NormallyOpen,
        SensorNumber = 2,
        DelayTimeSentUTC = DateTime.UtcNow,
        DescriptionSentUTC = DateTime.UtcNow,
        MonitorConditionSentUTC = DateTime.UtcNow,
        SensorConfigSentUTC = DateTime.UtcNow
      };
      data.CurrentDigitalRegistry.Sensors.Add(sensor2);

      //pending sensors
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>();
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test3",
        MonitorCondition = DigitalInputMonitoringConditions.Always,
        SensorConfiguration = InputConfig.NormallyClosed,
        SensorNumber = 3,
        DelayTimeSentUTC = oldestTimestamp,
        DescriptionSentUTC = DateTime.UtcNow,
        MonitorConditionSentUTC = DateTime.UtcNow,
        SensorConfigSentUTC = DateTime.UtcNow
      };
      data.PendingDigitalRegistry.Sensors.Add(sensor3);

      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = TimeSpan.FromHours(2.33345),
        Description = "Test4",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOffEngineOff,
        SensorConfiguration = InputConfig.NormallyOpen,
        SensorNumber = 4,
        DelayTimeSentUTC = oldestTimestamp,
        DescriptionSentUTC = DateTime.UtcNow,
        MonitorConditionSentUTC = DateTime.UtcNow,
        SensorConfigSentUTC = DateTime.UtcNow
      };
      data.PendingDigitalRegistry.Sensors.Add(sensor4);

      //general Current
      data.CurrentGeneralRegistry.BlockDataTransfer = false;
      data.CurrentGeneralRegistry.DataLinkType = "2";
      data.CurrentGeneralRegistry.GlobalGramEnable = true;
      data.CurrentGeneralRegistry.LastRegistrationDate = DateTime.UtcNow;
      data.CurrentGeneralRegistry.ModuleType = ModuleTypeEnum.PL121.ToString();
      data.CurrentGeneralRegistry.RegDealerCode = "6";
      data.CurrentGeneralRegistry.RegistrationStatus = "True";
      data.CurrentGeneralRegistry.RunTimeHoursAdj = TimeSpan.FromDays(1.234345);
      data.CurrentGeneralRegistry.StartStopEnable = false;
      data.CurrentGeneralRegistry.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule
      {
        ReportStartTime = TimeSpan.FromMinutes(2.11134),
        Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>()
      };
      var pos = new PLConfigData.GeneralRegistry.ReportingSchedule.Report
      {
        frequency = 4,
        ReportType = "Position",
        SentUTC = olderTimestamp
      };
      data.CurrentGeneralRegistry.ReportSchedule.Reports.Add(pos);
      var smu = new PLConfigData.GeneralRegistry.ReportingSchedule.Report
      {
        frequency = null,
        ReportType = "SMU",
        SentUTC = olderTimestamp
      };
      data.CurrentGeneralRegistry.ReportSchedule.Reports.Add(smu);
      data.CurrentGeneralRegistry.Software = new PLConfigData.GeneralRegistry.SoftwareInfo
      {
        HardwareSerialNumber = "1234",
        HC11SoftwarePartNumber = "12345",
        ModemSoftwarePartNumber = "123456",
        SoftwareRevision = "1234567"
      };
      data.CurrentGeneralRegistry.RuntimeHoursSentUTC = olderTimestamp;

      //general Pending
      data.PendingGeneralRegistry.BlockDataTransfer = false;
      data.PendingGeneralRegistry.DataLinkType = "2";
      data.PendingGeneralRegistry.GlobalGramEnable = true;
      data.PendingGeneralRegistry.LastRegistrationDate = DateTime.UtcNow;
      data.PendingGeneralRegistry.ModuleType = ModuleTypeEnum.PL300.ToString();
      data.PendingGeneralRegistry.RegDealerCode = "6";
      data.PendingGeneralRegistry.RegistrationStatus = "True";
      data.PendingGeneralRegistry.RunTimeHoursAdj = TimeSpan.FromDays(1.234345);
      data.PendingGeneralRegistry.StartStopEnable = false;
      data.PendingGeneralRegistry.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule
      {
        ReportStartTime = TimeSpan.FromMinutes(2.11134),
        Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>()
      };
      var pos1 = new PLConfigData.GeneralRegistry.ReportingSchedule.Report
      {
        frequency = 3,
        ReportType = "Position",
        SentUTC = olderTimestamp
      };
      data.PendingGeneralRegistry.ReportSchedule.Reports.Add(pos1);
      var smu1 = new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 2, ReportType = "SMU"};
      data.PendingGeneralRegistry.ReportSchedule.Reports.Add(smu1);
      data.PendingGeneralRegistry.Software = new PLConfigData.GeneralRegistry.SoftwareInfo
      {
        HardwareSerialNumber = "123",
        HC11SoftwarePartNumber = "1235",
        ModemSoftwarePartNumber = "12356",
        SoftwareRevision = "123567"
      };
      data.PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC = olderTimestamp;

      //Transmission Current
      data.CurrentTransmissionRegistry.EventIntervalHours = 2;
      data.CurrentTransmissionRegistry.NextMessageInterval = 22;
      data.CurrentTransmissionRegistry.SMUFuel = SMUFuelReporting.Fuel;
      data.CurrentTransmissionRegistry.EventReporting = new PLConfigData.TransmissionRegistry.EventReportingFrequency
      {
        DiagnosticFreqCode = EventFrequency.Immediately,
        Level1EventFreqCode = EventFrequency.Never,
        Level2EventFreqCode = EventFrequency.Next,
        Level3EventFreqCode = EventFrequency.Unknown
      };
      data.CurrentTransmissionRegistry.EventIntervalHoursSentUTC = olderTimestamp;

      //Transmission Pending
      data.PendingTransmissionRegistry.EventIntervalHours = 22;
      data.PendingTransmissionRegistry.NextMessageInterval = 23;
      data.PendingTransmissionRegistry.SMUFuel = SMUFuelReporting.Off;
      data.PendingTransmissionRegistry.EventReporting = new PLConfigData.TransmissionRegistry.EventReportingFrequency
      {
        DiagnosticFreqCode = EventFrequency.Unknown,
        Level1EventFreqCode = EventFrequency.Next,
        Level2EventFreqCode = EventFrequency.Never,
        Level3EventFreqCode = EventFrequency.Immediately
      };
      data.PendingTransmissionRegistry.EventIntervalHoursSentUTC = olderTimestamp;
      data.PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC = olderTimestamp;

      var newData = new PLConfigData(data.ToXElement());
      Assert.AreEqual(oldestTimestamp.KeyDate(), newData.OldestPendingKeyDate, "OldestPendingKeyDate is incorrect");
    }

    [Ignore]
    [TestMethod]
    public void PLConfigDataTest_NotConfiguredTestAllSensors()
    {
      var data = new PLConfigData {PendingDigitalRegistry = new PLConfigData.DigitalRegistry()};

      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation
                                                                 {SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1};
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 2 };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 4 };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>{sensor1, sensor2, sensor3, sensor4};
      var registry = new PLConfigData.DigitalRegistry();
      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current as not configured");
      Assert.IsNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should be null");
      Assert.AreEqual(InputConfig.NotConfigured, data.CurrentDigitalRegistry.Sensors[0].SensorConfiguration, "sensor 1 should be not configured");
      Assert.AreEqual(InputConfig.NotInstalled, data.CurrentDigitalRegistry.Sensors[1].SensorConfiguration, "sensor 2 should be not configured");
      Assert.AreEqual(InputConfig.NotConfigured, data.CurrentDigitalRegistry.Sensors[2].SensorConfiguration, "sensor 3 should be not configured");
      Assert.AreEqual(InputConfig.NotConfigured, data.CurrentDigitalRegistry.Sensors[3].SensorConfiguration, "sensor 4 should be not configured");
    }

    [TestMethod]
    [Ignore]
    public void PLConfigDataTest_NotConfiguredTestSomeSensors()
    {
      var data = new PLConfigData {PendingDigitalRegistry = new PLConfigData.DigitalRegistry()};

      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2 };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 4 };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };
      var registry = new PLConfigData.DigitalRegistry();
      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(3, data.CurrentDigitalRegistry.Sensors.Count, "3 sensors should be in current as not configured");
      Assert.IsNotNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should not be null");
      Assert.AreEqual(1, data.PendingDigitalRegistry.Sensors.Count, "incorrect number of sensors in pending");
      Assert.AreEqual(2, data.PendingDigitalRegistry.Sensors[0].SensorNumber, "only sensor 2 should be pending");
      Assert.AreEqual(InputConfig.NormallyClosed, data.PendingDigitalRegistry.Sensors[0].SensorConfiguration, " sensor 2 should be Normally closed");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual(InputConfig.NotConfigured, s1.SensorConfiguration, "sensor 1 should be not configured");
      Assert.AreEqual(InputConfig.NotInstalled, s3.SensorConfiguration, "sensor 3 should be not configured");
      Assert.AreEqual(InputConfig.NotConfigured, s4.SensorConfiguration, "sensor 4 should be not configured");
    }
    
    [Ignore]
    [TestMethod]
    public void PLConfigDataTest_NotConfiguredTestSomeSensorsAndCurrent()
    {
      var data = new PLConfigData
      {
        PendingDigitalRegistry = new PLConfigData.DigitalRegistry(),
        CurrentDigitalRegistry = new PLConfigData.DigitalRegistry()
      };
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 1};
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2, Description = "house"};
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 4 };
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      
      var sensor5 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor6 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2, Description = "Test" };
      var sensor7 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 3 };
      var sensor8 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor5, sensor6, sensor7, sensor8 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor9 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2, Description = "The" };
      var sensor10 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> {sensor9, sensor10};

      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current as not configured");
      Assert.IsNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should be null");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual(InputConfig.NormallyClosed, s2.SensorConfiguration, " sensor 2 should be Normally closed");
      Assert.AreEqual("The", s2.Description, " sensor 2 should be The");
      Assert.AreEqual(InputConfig.NotConfigured, s1.SensorConfiguration, "sensor 1 should be not configured");
      Assert.AreEqual(InputConfig.NotInstalled, s3.SensorConfiguration, "sensor 3 should be not configured");
      Assert.AreEqual(InputConfig.NormallyOpen, s4.SensorConfiguration, "sensor 4 should be NormallyOpen");
    }

    [TestMethod]
    [Ignore]
    public void PLConfigDataTest_NotConfiguredTestSomeSensorsAndCurrent1LeftPending()
    {
      var data = new PLConfigData
      {
        PendingDigitalRegistry = new PLConfigData.DigitalRegistry(),
        CurrentDigitalRegistry = new PLConfigData.DigitalRegistry()
      };
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 1 };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2, Description = "house" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 4 };
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      var sensor5 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor6 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 2, Description = "Test" };
      var sensor7 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 3 };
      var sensor8 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor5, sensor6, sensor7, sensor8 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor10 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor10 };

      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current as not configured");
      Assert.AreEqual(1, data.PendingDigitalRegistry.Sensors.Count, "1 sensors should be in Pending");
      Assert.IsNotNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should not be null");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual(InputConfig.NormallyClosed, s2.SensorConfiguration, " sensor 2 should be Normally closed");
      Assert.AreEqual("Test", s2.Description, " sensor 2 should be Test");
      Assert.AreEqual(InputConfig.NormallyClosed, s2.SensorConfiguration, " sensor 2 should be Normally closed");
      Assert.AreEqual("house", s2.Description, " sensor 2 should be Test");
      Assert.AreEqual(InputConfig.NotConfigured, s1.SensorConfiguration, "sensor 1 should be not configured");
      Assert.AreEqual(InputConfig.NotInstalled, s3.SensorConfiguration, "sensor 3 should be not configured");
      Assert.AreEqual(InputConfig.NormallyOpen, s4.SensorConfiguration, "sensor 4 should be NormallyOpen");
    }

    [TestMethod]
    [Ignore]
    public void PLConfigDataTest_NotUpdatingInputConfig()
    {
      var data = new PLConfigData
      {
        PendingDigitalRegistry = new PLConfigData.DigitalRegistry(),
        CurrentDigitalRegistry = new PLConfigData.DigitalRegistry()
      };
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 2, Description = "house" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 4, Description = "Q"};
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      var sensor5 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description =  "a"};
      var sensor6 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "Test" };
      var sensor7 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 3, Description = "b"};
      var sensor8 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 4, Description = "c"};
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor5, sensor6, sensor7, sensor8 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor10 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor10 };

      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current as not configured");
      Assert.AreEqual(2, data.PendingDigitalRegistry.Sensors.Count, "2 sensors should be in Pending");
      Assert.IsNotNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should not be null");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual(InputConfig.NotConfigured, s1.SensorConfiguration, " sensor 1 should be NotConfigured");
      Assert.AreEqual("a", s1.Description, " sensor 1 should be a");
      Assert.AreEqual(InputConfig.NotInstalled, s2.SensorConfiguration, " sensor 2 should be NotInstalled");
      Assert.AreEqual("Test", s2.Description, " sensor 2 should be a");
      Assert.AreEqual(InputConfig.NormallyClosed, s3.SensorConfiguration, " sensor 3 should be Normally closed");
      Assert.IsTrue(string.IsNullOrEmpty(s3.Description), " sensor 3 should be null");
      Assert.AreEqual(InputConfig.NormallyClosed, s4.SensorConfiguration, " sensor 4 should be Normally closed");
      Assert.AreEqual("Q", s4.Description, " sensor 4 should be Q");
      Assert.IsNull(s3.SensorConfiguration, " sensor 3 should be Normally closed");
      Assert.AreEqual("b", s3.Description, " sensor 3 should be b");
      Assert.IsNull(s4.SensorConfiguration, " sensor 4 should be Normally closed");
      Assert.AreEqual("c", s4.Description, " sensor 4 should be c");
    }

    [TestMethod]
    public void PLConfigDataTest_NOPending()
    {
      var data = new PLConfigData {CurrentDigitalRegistry = new PLConfigData.DigitalRegistry()};
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 2, Description = "house" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 4, Description = "Q" };
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor10 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyOpen, SensorNumber = 4 };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor10 };

      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current as not configured");
      Assert.IsNull(data.PendingDigitalRegistry, "Pending DigitalRegistry should be null");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual(InputConfig.NotConfigured, s1.SensorConfiguration, " sensor 1 should be NotConfigured");
      Assert.AreEqual(InputConfig.NotInstalled, s2.SensorConfiguration, " sensor 2 should be NotInstalled");
      Assert.AreEqual("house", s2.Description, " sensor 2 should be a");
      Assert.AreEqual(InputConfig.NormallyClosed, s3.SensorConfiguration, " sensor 3 should be Normally closed");
      Assert.IsTrue(string.IsNullOrEmpty(s3.Description), " sensor 3 should be null");
      Assert.AreEqual(InputConfig.NormallyOpen, s4.SensorConfiguration, " sensor 4 should be Normally Open");
    }

    [TestMethod]
    public void PLConfigDataTest_NOPendingUnknownConfig()
    {
      var data = new PLConfigData {CurrentDigitalRegistry = new PLConfigData.DigitalRegistry()};
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotConfigured, SensorNumber = 1 };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NotInstalled, SensorNumber = 2, Description = "house" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 3 };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorConfiguration = InputConfig.NormallyClosed, SensorNumber = 4, Description = "Q" };
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      var registry = new PLConfigData.DigitalRegistry();

      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.IsNull(data.CurrentDigitalRegistry.Sensors, "0 sensors should be in current");
    }

    [Ignore]
    [TestMethod]
    public void PLConfigDataTest_NOCurrentNoConfig()
    {
      var data = new PLConfigData {PendingDigitalRegistry = new PLConfigData.DigitalRegistry()};
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description = "1"};
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "2" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 3, Description = "3"};
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 4, Description = "4" };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor5 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description = "5" };
      var sensor6 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "6" };
      var sensor7 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 3, Description = "7" };
      var sensor8 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 4, Description = "8" };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor5, sensor6, sensor7, sensor8 };


      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current");
      Assert.IsNull(data.PendingDigitalRegistry, "0 sensors should be in pending");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual("5", s1.Description, " sensor 1 should be 5");
      Assert.AreEqual("6", s2.Description, " sensor 2 should be 6");
      Assert.AreEqual("7", s3.Description, " sensor 3 should be 7");
      Assert.AreEqual("8", s4.Description, " sensor 4 should be 8");
    }

    [Ignore]
    [TestMethod]
    public void PLConfigDataTest_SomeCurrentNoConfig()
    {
      var data = new PLConfigData {PendingDigitalRegistry = new PLConfigData.DigitalRegistry()};
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description = "1" };
      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "2" };
      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 3, Description = "3" };
      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 4, Description = "4" };
      data.PendingDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor1, sensor2, sensor3, sensor4 };

      data.CurrentDigitalRegistry = new PLConfigData.DigitalRegistry();
      var sensor9 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description = "9" };
      var sensor10 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "10" };
      data.CurrentDigitalRegistry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor9, sensor10 };

      var registry = new PLConfigData.DigitalRegistry();
      var sensor5 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 1, Description = "5" };
      var sensor6 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 2, Description = "6" };
      var sensor7 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 3, Description = "7" };
      var sensor8 = new PLConfigData.DigitalRegistry.SensorInformation { SensorNumber = 4, Description = "8" };
      registry.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation> { sensor5, sensor6, sensor7, sensor8 };


      data.Update(registry, MessageStatusEnum.Acknowledged);
      Assert.AreEqual(4, data.CurrentDigitalRegistry.Sensors.Count, "4 sensors should be in current");
      Assert.IsNull(data.PendingDigitalRegistry, "0 sensors should be in pending");

      var s1 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 1);
      Assert.IsNotNull(s1);
      var s2 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 2);
      Assert.IsNotNull(s2);
      var s3 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 3);
      Assert.IsNotNull(s3);
      var s4 = data.CurrentDigitalRegistry.Sensors.FirstOrDefault(e => e.SensorNumber == 4);
      Assert.IsNotNull(s4);

      Assert.AreEqual("5", s1.Description, " sensor 1 should be 5");
      Assert.AreEqual("6", s2.Description, " sensor 2 should be 6");
      Assert.AreEqual("7", s3.Description, " sensor 3 should be 7");
      Assert.AreEqual("8", s4.Description, " sensor 4 should be 8");
    }

    [TestMethod]
    public void PLConfigDataTestOneRegistry()
    {
      var data = new PLConfigData
      {
        PendingTransmissionRegistry =
          new PLConfigData.TransmissionRegistry
          {
            EventIntervalHours = 22,
            NextMessageInterval = 23,
            SMUFuel = SMUFuelReporting.Off,
            EventReporting =
              new PLConfigData.TransmissionRegistry.EventReportingFrequency
              {
                DiagnosticFreqCode = EventFrequency.Unknown,
                Level1EventFreqCode = EventFrequency.Next,
                Level2EventFreqCode = EventFrequency.Never,
                Level3EventFreqCode = EventFrequency.Immediately
              }
          }
      };

      //Transmission Pending

      var newData = new PLConfigData(data.ToXElement().ToString());
      Assert.AreEqual(data.ToXElement().ToString(), newData.ToXElement().ToString(), "Incorrect XML");
    }

    [TestMethod]
    public void GeneralRegistryTest()
    {
      var general = new PLConfigData.GeneralRegistry
      {
        BlockDataTransfer = true,
        DataLinkType = "2",
        GlobalGramEnable = false,
        LastRegistrationDate = DateTime.UtcNow,
        ModuleType = ModuleTypeEnum.PL121.ToString(),
        RegDealerCode = "27",
        RegistrationStatus = "10",
        ReportSchedule =
          new PLConfigData.GeneralRegistry.ReportingSchedule
          {
            Reports =
              new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>
              {
                new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 3, ReportType = "Position"},
                new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 2, ReportType = "SMU"},
                new PLConfigData.GeneralRegistry.ReportingSchedule.Report {frequency = 1, ReportType = "Pos"}
              },
            ReportStartTime = new TimeSpan(12, 20, 0)
          },
        RunTimeHoursAdj = TimeSpan.FromMinutes(3.12112212121),
        Software = new PLConfigData.GeneralRegistry.SoftwareInfo
        {
          SoftwareRevision = "12",
          HardwareSerialNumber = "343",
          HC11SoftwarePartNumber = "asdf"
        }
      };
      general.Software.HardwareSerialNumber = "qwerty";
      general.Software.ModemSoftwarePartNumber = "lkjhg";
      general.StartStopEnable = false;

      var newGeneral = new PLConfigData.GeneralRegistry(general.ToXElement());
      Assert.AreEqual(general.BlockDataTransfer, newGeneral.BlockDataTransfer, "Incorrect BlockDataTransfer");
      Assert.AreEqual(general.DataLinkType, newGeneral.DataLinkType, "Incorrect DataLinkType");
      Assert.AreEqual(general.GlobalGramEnable, newGeneral.GlobalGramEnable, "Incorrect GlobalGramEnable");
      Assert.AreEqual(general.LastRegistrationDate, newGeneral.LastRegistrationDate, "Incorrect LastRegistrationDate");
      Assert.AreEqual(general.ModuleType, newGeneral.ModuleType, "Incorrect ModuleType");
      Assert.AreEqual(general.RegDealerCode, newGeneral.RegDealerCode, "Incorrect RegDealerCode");
      Assert.AreEqual(general.RegistrationStatus, newGeneral.RegistrationStatus, "Incorrect RegistrationStatus");
      for (var i = 0; i < 3; i++)
      {
        Assert.AreEqual(general.ReportSchedule.Reports[i].frequency, newGeneral.ReportSchedule.Reports[i].frequency, string.Format("Incorrect ReportFrequency at {0}", i));
        Assert.AreEqual(general.ReportSchedule.Reports[i].ReportType, newGeneral.ReportSchedule.Reports[i].ReportType, string.Format("Incorrect ReportType at {0}", i));
      }
      Assert.AreEqual(general.ReportSchedule.ReportStartTime, newGeneral.ReportSchedule.ReportStartTime, "Incorrect ReportStartTime");
      Assert.AreEqual(general.RunTimeHoursAdj, newGeneral.RunTimeHoursAdj, "Incorrect RunTimeHoursAdj");
      Assert.AreEqual(general.Software.HardwareSerialNumber, newGeneral.Software.HardwareSerialNumber, "Incorrect HardwareSerialNumber");
      Assert.AreEqual(general.Software.HC11SoftwarePartNumber, newGeneral.Software.HC11SoftwarePartNumber, "Incorrect HC11SoftwarePartNumber");
      Assert.AreEqual(general.Software.ModemSoftwarePartNumber, newGeneral.Software.ModemSoftwarePartNumber, "Incorrect ModemSoftwarePartNumber");
      Assert.AreEqual(general.Software.SoftwareRevision, newGeneral.Software.SoftwareRevision, "Incorrect SoftwareRevision");
      Assert.AreEqual(general.StartStopEnable, newGeneral.StartStopEnable, "Incorrect StartStopEnable");
    }

    [TestMethod]
    public void TransmissionRegistryTest()
    {
      var transmission = new PLConfigData.TransmissionRegistry
      {
        EventIntervalHours = 3,
        EventReporting =
          new PLConfigData.TransmissionRegistry.EventReportingFrequency
          {
            DiagnosticFreqCode = EventFrequency.Immediately,
            Level1EventFreqCode = EventFrequency.Never,
            Level2EventFreqCode = EventFrequency.Next,
            Level3EventFreqCode = EventFrequency.Unknown
          },
        NextMessageInterval = 2,
        SMUFuel = SMUFuelReporting.SMUFUEL
      };

      var newTransmission = new PLConfigData.TransmissionRegistry(transmission.ToXElement());
      Assert.AreEqual(transmission.EventIntervalHours, newTransmission.EventIntervalHours, "Incorrect EventIntervalHours");
      Assert.AreEqual(transmission.EventReporting.DiagnosticFreqCode, newTransmission.EventReporting.DiagnosticFreqCode, "Incorrect DiagnosticFreqCode");
      Assert.AreEqual(transmission.EventReporting.Level1EventFreqCode, newTransmission.EventReporting.Level1EventFreqCode, "Incorrect Level1EventFreqCode");
      Assert.AreEqual(transmission.EventReporting.Level2EventFreqCode, newTransmission.EventReporting.Level2EventFreqCode, "Incorrect Level2EventFreqCode");
      Assert.AreEqual(transmission.EventReporting.Level3EventFreqCode, newTransmission.EventReporting.Level3EventFreqCode, "Incorrect Level3EventFreqCode");
      Assert.AreEqual(transmission.NextMessageInterval, newTransmission.NextMessageInterval, "Incorrect NextMessageInterval");
      Assert.AreEqual(transmission.SMUFuel, newTransmission.SMUFuel, "Incorrect SMUFuel");
    }

    [TestMethod]
    public void DigitalRegistryTest()
    {
      var digital = new PLConfigData.DigitalRegistry
      {
        Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>()
      };
      var sensor1 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = new TimeSpan(1, 2, 0),
        Description = "Test1",
        MonitorCondition = DigitalInputMonitoringConditions.Always,
        SensorConfiguration = InputConfig.NormallyClosed,
        SensorNumber = 1
      };
      digital.Sensors.Add(sensor1);

      var sensor2 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = new TimeSpan(2, 2, 0),
        Description = "Test2",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOffEngineOff,
        SensorConfiguration = InputConfig.NormallyOpen,
        SensorNumber = 2
      };
      digital.Sensors.Add(sensor2);

      var sensor3 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = new TimeSpan(3, 2, 0),
        Description = "Test3",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOnEngineOff,
        SensorConfiguration = InputConfig.NotConfigured,
        SensorNumber = 3
      };
      digital.Sensors.Add(sensor3);

      var sensor4 = new PLConfigData.DigitalRegistry.SensorInformation
      {
        DelayTime = new TimeSpan(4, 2, 0),
        Description = "Test4",
        MonitorCondition = DigitalInputMonitoringConditions.KeyOnEngineOn,
        SensorConfiguration = InputConfig.NotInstalled,
        SensorNumber = 4
      };
      digital.Sensors.Add(sensor4);

      var newDigital = new PLConfigData.DigitalRegistry(digital.ToXElement());
      Assert.AreEqual(digital.Sensors.Count, newDigital.Sensors.Count, "Incorrect Number of Sensors");
      for (var i = 0; i < digital.Sensors.Count; i++)
      {
        Assert.AreEqual(digital.Sensors[i].DelayTime, newDigital.Sensors[i].DelayTime, string.Format("Incorrect Delay Time at {0}", i));
        Assert.AreEqual(digital.Sensors[i].Description, newDigital.Sensors[i].Description, string.Format("Incorrect Description at {0}", i));
        Assert.AreEqual(digital.Sensors[i].MonitorCondition, newDigital.Sensors[i].MonitorCondition, string.Format("Incorrect MonitorCondition at {0}", i));
        Assert.AreEqual(digital.Sensors[i].SensorConfiguration, newDigital.Sensors[i].SensorConfiguration, string.Format("Incorrect SensorConfiguration at {0}", i));
        Assert.AreEqual(digital.Sensors[i].SensorNumber, newDigital.Sensors[i].SensorNumber, string.Format("Incorrect SensorNumber at {0}", i));
      }
    }

    [TestMethod]
    public void Update_PopulatesPending()
    {
      var data = new PLConfigData();

      var general = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);
      var transmission = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);
      var digital = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      for (var sensorNumber = 0; sensorNumber < digital.Sensors.Count; sensorNumber++)
        digital.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique

      data.Update(general, MessageStatusEnum.Pending);
      data.Update(transmission, MessageStatusEnum.Pending);
      data.Update(digital, MessageStatusEnum.Pending);

      Assert.IsNotNull(data.PendingGeneralRegistry, "Pending general registry should be populated");
      Assert.IsNotNull(data.PendingTransmissionRegistry, "Pending transmission registry should be populated");
      Assert.IsNotNull(data.PendingDigitalRegistry, "Pending digital registry should be populated");
    }

    [TestMethod]
    public void Update_ClearsPendingWhenEverythingAcknowledged()
    {
      var data = new PLConfigData();
      var general1 = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);
      var transmission1 = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);
      var digital1 = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      var general2 = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);
      var transmission2 = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);
      var digital2 = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      for (var sensorNumber = 0; sensorNumber < digital1.Sensors.Count; sensorNumber++)
        digital1.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique
      for (var sensorNumber = 0; sensorNumber < digital2.Sensors.Count; sensorNumber++)
        digital2.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique

      data.Update(general1, MessageStatusEnum.Pending);
      data.Update(transmission1, MessageStatusEnum.Pending);
      data.Update(digital1, MessageStatusEnum.Pending);
      data.Update(general2, MessageStatusEnum.Acknowledged);
      data.Update(transmission2, MessageStatusEnum.Acknowledged);
      data.Update(digital2, MessageStatusEnum.Acknowledged);

      Assert.IsNull(data.PendingGeneralRegistry, "Pending general registry should be cleared");
      Assert.IsNull(data.PendingTransmissionRegistry, "Pending transmission registry should be cleared");
      Assert.IsNull(data.PendingDigitalRegistry, "Pending digital registry should be cleared");
      Assert.IsNotNull(data.CurrentGeneralRegistry, "Current general registry should be populated");
      Assert.IsNotNull(data.CurrentTransmissionRegistry, "Current transmission registry should be populated");
      Assert.IsNotNull(data.CurrentDigitalRegistry, "Current digital registry should be populated");
    }

    [TestMethod]
    public void Update_GeneralPartiallyAcknowledged()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);

      data.Update(config, MessageStatusEnum.Pending);
      config.ReportSchedule.Reports[0].frequency += 1;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since ReportSchedule.Reports[0].frequency is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.ReportSchedule.Reports[0].ReportType += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since ReportSchedule.Reports[0].ReportType is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.ReportSchedule.ReportStartTime += TimeSpan.FromMinutes(1);
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since ReportSchedule.ReportStartTime is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Software.HardwareSerialNumber += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since Software.HardwareSerialNumber is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Software.HC11SoftwarePartNumber += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since Software.HC11SoftwarePartNumber is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Software.ModemSoftwarePartNumber += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since Software.ModemSoftwarePartNumber is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Software.SoftwareRevision += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since Software.SoftwareRevision is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.BlockDataTransfer = !config.BlockDataTransfer;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since BlockDataTransfer is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.DataLinkType += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since DataLinkType is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.GlobalGramEnable = !config.GlobalGramEnable;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since GlobalGramEnable is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.ModuleType += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since ModuleType is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.RegDealerCode += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since RegDealerCode is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.RegistrationStatus += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since RegistrationStatus is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.RunTimeHoursAdj += TimeSpan.FromMinutes(1);
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since RunTimeHoursAdj is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.StartStopEnable = !config.StartStopEnable;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since StartStopEnable is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.ReportSchedule.Reports.RemoveAt(0);
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingGeneralRegistry, "There should still be pending changes in the General registry since ReportSchedule.Reports is different length");
    }

    [TestMethod]
    public void Update_TransmissionPartiallyAcknowledged()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);

      data.Update(config, MessageStatusEnum.Pending);
      config.EventIntervalHours += 1;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since EventIntervalHours is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.NextMessageInterval += 1;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since NextMessageInterval is different");

      config.SMUFuel = SMUFuelReporting.Fuel;
      data.Update(config, MessageStatusEnum.Pending);
      config.SMUFuel = SMUFuelReporting.SMU;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since SMUFuel is different");

      config.EventReporting.DiagnosticFreqCode = EventFrequency.Immediately;
      data.Update(config, MessageStatusEnum.Pending);
      config.EventReporting.DiagnosticFreqCode = EventFrequency.Next;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since EventReporting.DiagnosticFreqCode is different");

      config.EventReporting.Level1EventFreqCode = EventFrequency.Immediately;
      data.Update(config, MessageStatusEnum.Pending);
      config.EventReporting.Level1EventFreqCode = EventFrequency.Next;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since EventReporting.Level1EventFreqCode is different");

      config.EventReporting.Level2EventFreqCode = EventFrequency.Immediately;
      data.Update(config, MessageStatusEnum.Pending);
      config.EventReporting.Level2EventFreqCode = EventFrequency.Next;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since EventReporting.Level2EventFreqCode is different");

      config.EventReporting.Level3EventFreqCode = EventFrequency.Immediately;
      data.Update(config, MessageStatusEnum.Pending);
      config.EventReporting.Level3EventFreqCode = EventFrequency.Next;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingTransmissionRegistry, "There should still be pending changes in the Transmission registry since EventReporting.Level3EventFreqCode is different");
    }

    [TestMethod]
    public void Update_DigitalPartiallyAcknowledged()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      for (var sensorNumber = 0; sensorNumber < config.Sensors.Count; sensorNumber++ )
        config.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique

      data.Update(config, MessageStatusEnum.Pending);
      config.Sensors[0].DelayTime += TimeSpan.FromMinutes(1);
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingDigitalRegistry, "There should still be pending changes in the Digital registry since  is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Sensors[0].Description += "-changed";
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingDigitalRegistry, "There should still be pending changes in the Digital registry since  is different");

      config.Sensors[0].MonitorCondition = DigitalInputMonitoringConditions.Always;
      data.Update(config, MessageStatusEnum.Pending);
      config.Sensors[0].MonitorCondition = DigitalInputMonitoringConditions.KeyOnEngineOff;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingDigitalRegistry, "There should still be pending changes in the Digital registry since  is different");

      config.Sensors[0].SensorConfiguration = InputConfig.NormallyClosed;
      data.Update(config, MessageStatusEnum.Pending);
      config.Sensors[0].SensorConfiguration = InputConfig.NormallyOpen;
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingDigitalRegistry, "There should still be pending changes in the Digital registry since  is different");

      data.Update(config, MessageStatusEnum.Pending);
      config.Sensors.RemoveAt(0);
      data.Update(config, MessageStatusEnum.Acknowledged);
      Assert.IsNotNull(data.PendingDigitalRegistry, "There should still be pending changes in the Digital registry since Sensors is different length");
    }

    [TestMethod]
    public void CleanupStalePendingRegistryEntries_General()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.GeneralRegistry>().Generate(Seed);

      var cleanupOlderThanKeyDate = DateTime.UtcNow.AddDays(-7).KeyDate();
      var tooOld = DateTime.UtcNow.AddDays(-9);

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingGeneralRegistry.GlobalGramSentUTC = tooOld;
      Assert.IsNotNull(data.PendingGeneralRegistry.GlobalGramEnable);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingGeneralRegistry.GlobalGramEnable, "GlobalGramEnable should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingGeneralRegistry.RuntimeHoursSentUTC = tooOld;
      Assert.IsNotNull(data.PendingGeneralRegistry.RunTimeHoursAdj);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingGeneralRegistry.RunTimeHoursAdj, "RunTimeHoursAdj should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingGeneralRegistry.StartStopEnableSentUTC = tooOld;
      Assert.IsNotNull(data.PendingGeneralRegistry.StartStopEnable);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingGeneralRegistry.StartStopEnable, "StartStopEnable should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC = tooOld;
      Assert.IsNotNull(data.PendingGeneralRegistry.ReportSchedule.ReportStartTime);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingGeneralRegistry.ReportSchedule.ReportStartTime, "ReportSchedule.ReportStartTime should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      var report = data.PendingGeneralRegistry.ReportSchedule.Reports[0];
      report.SentUTC = tooOld;
      CollectionAssert.Contains(data.PendingGeneralRegistry.ReportSchedule.Reports, report);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      CollectionAssert.DoesNotContain(data.PendingGeneralRegistry.ReportSchedule.Reports, report, "Report should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC = tooOld;
      foreach (var r in data.PendingGeneralRegistry.ReportSchedule.Reports)
        r.SentUTC = tooOld;
      Assert.IsNotNull(data.PendingGeneralRegistry.ReportSchedule);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingGeneralRegistry.ReportSchedule, "PendingGeneralRegistry.ReportSchedule should have been removed since it is stale");

    }

    [TestMethod]
    public void CleanupStalePendingRegistryEntries_Transmission()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.TransmissionRegistry>().Generate(Seed);

      var cleanupOlderThanKeyDate = DateTime.UtcNow.AddDays(-7).KeyDate();
      var tooOld = DateTime.UtcNow.AddDays(-9);

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.SMUFuelSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.SMUFuel);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.SMUFuel, "SMUFuel should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.NextMessageIntervalSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.NextMessageInterval);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.NextMessageInterval, "NextMessageInterval should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.EventIntervalHoursSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.EventIntervalHours);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.EventIntervalHours, "EventIntervalHours should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.EventReporting.DiagnosticFreqCode);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.EventReporting.DiagnosticFreqCode, "EventReporting.DiagnosticFreqCode should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.EventReporting.Level1EventFreqCode);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.EventReporting.Level1EventFreqCode, "EventReporting.Level1EventFreqCode should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.EventReporting.Level2EventFreqCode);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.EventReporting.Level2EventFreqCode, "EventReporting.Level2EventFreqCode should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC = tooOld;
      Assert.IsNotNull(data.PendingTransmissionRegistry.EventReporting.Level3EventFreqCode);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingTransmissionRegistry.EventReporting.Level3EventFreqCode, "EventReporting.Level3EventFreqCode should have been removed since it is stale");
    }

    [TestMethod]
    public void CleanupStalePendingRegistryEntries_Digital()
    {
      var data = new PLConfigData();
      var config = new ObjectGenerator<PLConfigData.DigitalRegistry>().Generate(Seed);
      for (var sensorNumber = 0; sensorNumber < config.Sensors.Count; sensorNumber++)
        config.Sensors[sensorNumber].SensorNumber = sensorNumber + 1; // ensure sensor number is unique

      var cleanupOlderThanKeyDate= DateTime.UtcNow.AddDays(-7).KeyDate();
      var tooOld = DateTime.UtcNow.AddDays(-9);

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingDigitalRegistry.Sensors[0].DelayTimeSentUTC = tooOld;
      Assert.IsNotNull(data.PendingDigitalRegistry.Sensors[0].DelayTime);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingDigitalRegistry.Sensors[0].DelayTime, "DelayTime should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingDigitalRegistry.Sensors[0].DescriptionSentUTC = tooOld;
      Assert.IsNotNull(data.PendingDigitalRegistry.Sensors[0].Description);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingDigitalRegistry.Sensors[0].Description, "Description should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingDigitalRegistry.Sensors[0].MonitorConditionSentUTC = tooOld;
      Assert.IsNotNull(data.PendingDigitalRegistry.Sensors[0].MonitorCondition);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingDigitalRegistry.Sensors[0].MonitorCondition, "StartStopEnable should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      data.PendingDigitalRegistry.Sensors[0].SensorConfigSentUTC = tooOld;
      Assert.IsNotNull(data.PendingDigitalRegistry.Sensors[0].SensorConfiguration);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      Assert.IsNull(data.PendingDigitalRegistry.Sensors[0].SensorConfiguration, "SensorConfiguration should have been removed since it is stale");

      data.Update(config, MessageStatusEnum.Pending);
      var sensor = data.PendingDigitalRegistry.Sensors[0];
      sensor.DelayTimeSentUTC = tooOld;
      sensor.DescriptionSentUTC = tooOld;
      sensor.MonitorConditionSentUTC = tooOld;
      sensor.SensorConfigSentUTC = tooOld;
      CollectionAssert.Contains(data.PendingDigitalRegistry.Sensors, sensor);
      data.CleanupStalePendingRegistryEntries(cleanupOlderThanKeyDate);
      CollectionAssert.DoesNotContain(data.PendingDigitalRegistry.Sensors, sensor, "Sensor should have been removed since it is stale");
    }

    [TestMethod]
    public void TestInvalidSensorConfigurationCode()
    {
      const string xml = @"
<DigitalRegistry>
 <sensorInformation>
  <sensorNumber>1</sensorNumber>
  <sensorConfigurationCode>57</sensorConfigurationCode>
  <delayTimeSec>6534</delayTimeSec>
  <userDescription>DIGITAL INPUT 1</userDescription>
  <monitorCondition>028F</monitorCondition>
 </sensorInformation>
 <sensorInformation>
  <sensorNumber>2</sensorNumber>
  <sensorConfigurationCode>59</sensorConfigurationCode>
  <delayTimeSec>6534</delayTimeSec>
  <userDescription>DIGITAL INPUT 2</userDescription>
  <monitorCondition>028F</monitorCondition>
 </sensorInformation>
</DigitalRegistry>";
      var digitalRegistry = XElement.Parse(xml);
      var newDigital = new PLConfigData.DigitalRegistry(digitalRegistry);
      Assert.IsNotNull(newDigital);
      Assert.AreEqual(2, newDigital.Sensors.Count);
      var goodSensor = newDigital.Sensors[0];
      var badSensor = newDigital.Sensors[1];
      Assert.AreEqual(InputConfig.NormallyOpen, goodSensor.SensorConfiguration); // 57
      Assert.IsNull(badSensor.SensorConfiguration); // 59 does not correspond to any enum value
    }

    [TestMethod]
    public void TestInvalidMonitoringCondition()
    {
      const string xml = @"
<DigitalRegistry>
 <sensorInformation>
  <sensorNumber>1</sensorNumber>
  <sensorConfigurationCode>57</sensorConfigurationCode>
  <delayTimeSec>6534</delayTimeSec>
  <userDescription>DIGITAL INPUT 1</userDescription>
  <monitorCondition>028F</monitorCondition>
 </sensorInformation>
 <sensorInformation>
  <sensorNumber>2</sensorNumber>
  <sensorConfigurationCode>58</sensorConfigurationCode>
  <delayTimeSec>6534</delayTimeSec>
  <userDescription>DIGITAL INPUT 2</userDescription>
  <monitorCondition>023F</monitorCondition>
 </sensorInformation>
</DigitalRegistry>";
      var digitalRegistry = XElement.Parse(xml);
      var newDigital = new PLConfigData.DigitalRegistry(digitalRegistry);
      Assert.IsNotNull(newDigital);
      Assert.AreEqual(2, newDigital.Sensors.Count);
      var goodSensor = newDigital.Sensors[0];
      var badSensor = newDigital.Sensors[1];
      Assert.AreEqual(DigitalInputMonitoringConditions.KeyOnEngineOn, goodSensor.MonitorCondition); // 028F
      Assert.IsNull(badSensor.MonitorCondition); // 023F does not correspond to any enum value
    }
  }
}
