using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.PLMessages;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
    /// <summary>
    /// Summary description for OTAConfigMessagesTest
    /// </summary>
    [TestClass]
    public class OTAConfigMessagesTest
    {
        public OTAConfigMessagesTest()
        {
         
        }

        [TestMethod]
        public void OTAConfigMessagesParseTest()
        {
          uint RuntimeHoursAdj = 100;
          ushort eventIntervalHours = 10;
          string Level1Frequency = "Immediately";
          string Level2Frequency = "Immediately";
          string Level3Frequency = "Immediately";
          ushort nextMessageIntervalHours = 10;
          bool globalGramEnable = false;
          string reportStartTime = new DateTime(2009, 12, 2, 9, 2, 0).ToString();//"12/2/2009 9:02:00 AM";
          string DiagnosticTransmissionFrequency = "Immediately";
          string SmuFuelReporting = "Fuel";
          bool startStopConfiguration = true;
          byte positionReportConfig = 1;   //report frequency

          PL321OTAConfigMessages ota = new PL321OTAConfigMessages();
          ota.RuntimeHoursAdj = TimeSpan.FromHours(100);
          ota.EventIntervals = TimeSpan.FromHours(10);
          ota.Level1Frequency = EventFrequency.Immediately;
          ota.Level2Frequency = EventFrequency.Immediately;
          ota.Level3Frequency = EventFrequency.Immediately;
          ota.NextMessageInterval = TimeSpan.FromHours(10);
          ota.GlobalGramEnable = false;
          ota.ReportStartTimeUTC = new DateTime(2009, 12, 2, 9, 2, 0);//"12/2/2009 9:02:00 AM";
          ota.DiagnosticTransmissionFrequency = EventFrequency.Immediately;
          ota.SmuFuelReporting = SMUFuelReporting.Fuel;
          ota.StartStopConfigurationEnable = true;
          ota.PositionReportConfig = 1;
          uint bitPosition = 0;
          byte[] rptMessage = PLBaseMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);

          PL321OTAConfigMessages cngfMessage = PLMessageBase.HydratePLMessageBase(PLMessageBase.BytesToBinaryString(rptMessage), false) as PL321OTAConfigMessages;

          Assert.AreEqual((ushort)cngfMessage.RuntimeHoursAdj.Value.TotalHours, RuntimeHoursAdj, "Parse failed for RuntimeHours");
          Assert.AreEqual((ushort)cngfMessage.EventIntervals.Value.TotalHours, eventIntervalHours, "Parse failed for RuntimeHours");
          Assert.AreEqual(cngfMessage.Level1Frequency.ToString(), Level1Frequency, "Parse failed for Level1Frequency");
          Assert.AreEqual(cngfMessage.Level2Frequency.ToString(), Level2Frequency, "Parse failed for Level2Frequency");
          Assert.AreEqual(cngfMessage.Level3Frequency.ToString(), Level3Frequency, "Parse failed for Level3Frequency");
          Assert.AreEqual((ushort)cngfMessage.NextMessageInterval.Value.TotalHours, nextMessageIntervalHours, "Parse failed for nextMessageIntervalHours");
          Assert.AreEqual(cngfMessage.GlobalGramEnable, globalGramEnable, "Parse failed for globalGramEnable");
          Assert.AreEqual(cngfMessage.ReportStartTimeUTC.ToString(), reportStartTime, "Parse failed for ReportStartTime");
          Assert.AreEqual(cngfMessage.DiagnosticTransmissionFrequency.ToString(), DiagnosticTransmissionFrequency, "Parse failed for RuntimeHours");
          Assert.AreEqual(cngfMessage.SmuFuelReporting.Value.ToString(), SmuFuelReporting, "Parse failed for SmuFuelReporting");
          Assert.AreEqual(cngfMessage.StartStopConfigurationEnable.Value, startStopConfiguration, "Parse failed for StartStopConfiguration");
          Assert.AreEqual(cngfMessage.PositionReportConfig.Value, positionReportConfig, "Parse failed for ReportFrequence");
        }

        [TestMethod]
        public void DigitalInputParseTest()
        {
          PL321OTAConfigMessages guzinta = new PL321OTAConfigMessages();
          guzinta.DigitalInput1MonitoringCondition = DigitalInputMonitoringConditions.Always;
          guzinta.DigitalInput2MonitoringCondition = DigitalInputMonitoringConditions.KeyOffEngineOff;
          guzinta.DigitalInput3MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOff;
          guzinta.DigitalInput4MonitoringCondition = DigitalInputMonitoringConditions.KeyOnEngineOn;
          guzinta.Input1DelayTime = new TimeSpan(0,0,0,5);
          guzinta.Input2DelayTime = new TimeSpan(0, 0, 0, 10);
          guzinta.Input3DelayTime = new TimeSpan(0, 0, 0, 15);
          guzinta.Input4DelayTime = new TimeSpan(0, 0, 0, 20);
          guzinta.InputConfig1 = InputConfig.NormallyClosed;
          guzinta.InputConfig2 = InputConfig.NormallyOpen;
          guzinta.InputConfig3 = InputConfig.NotConfigured;
          guzinta.InputConfig4 = InputConfig.NotInstalled;

          uint bitPosition = 0;
          byte[] rptMessage = PLBaseMessage.SerializePlatformMessage(guzinta, null, ref bitPosition, false);

          PL321OTAConfigMessages comezouta = PLMessageBase.HydratePLMessageBase(PLMessageBase.BytesToBinaryString(rptMessage), false) as PL321OTAConfigMessages;

          Assert.AreEqual(comezouta.DigitalInput1MonitoringCondition.Value, guzinta.DigitalInput1MonitoringCondition.Value, "Parse failed for DigitalInput1MonitoringCondition");
          Assert.AreEqual(comezouta.DigitalInput2MonitoringCondition.Value, guzinta.DigitalInput2MonitoringCondition.Value, "Parse failed for DigitalInput2MonitoringCondition");
          Assert.AreEqual(comezouta.DigitalInput3MonitoringCondition.Value, guzinta.DigitalInput3MonitoringCondition.Value, "Parse failed for DigitalInput3MonitoringCondition");
          Assert.AreEqual(comezouta.DigitalInput4MonitoringCondition.Value, guzinta.DigitalInput4MonitoringCondition.Value, "Parse failed for DigitalInput4MonitoringCondition");
          Assert.AreEqual(comezouta.Input1DelayTime.Value, guzinta.Input1DelayTime.Value, "Parse failed for Input1DelayTime");
          Assert.AreEqual(comezouta.Input2DelayTime.Value, guzinta.Input2DelayTime.Value, "Parse failed for Input2DelayTime");
          Assert.AreEqual(comezouta.Input3DelayTime.Value, guzinta.Input3DelayTime.Value, "Parse failed for Input3DelayTime");
          Assert.AreEqual(comezouta.Input4DelayTime.Value, guzinta.Input4DelayTime.Value, "Parse failed for Input4DelayTime");
          Assert.AreEqual(comezouta.InputConfig1.Value, guzinta.InputConfig1.Value, "Parse failed for InputConfig1");
          Assert.AreEqual(comezouta.InputConfig2.Value, guzinta.InputConfig2.Value, "Parse failed for InputConfig2");
          Assert.AreEqual(comezouta.InputConfig3.Value, guzinta.InputConfig3.Value, "Parse failed for InputConfig3");
          Assert.AreEqual(comezouta.InputConfig4.Value, guzinta.InputConfig4.Value, "Parse failed for InputConfig4");
        }
    }
}