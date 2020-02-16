using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.PLMessages;
using VSS.Hosted.VLCommon.MTSMessages;


namespace VSS.Hosted.VLCommon
{
  public static class PLOutboundFormatter
  {
    public static byte[] FormatFenceConfig(bool inclusiveProductWatch, bool exclusiveProductWatch, bool timeBasedProductWatch, byte inclusiveMessageID,
      decimal inclusiveLatitude, decimal inclusiveLongitude, decimal inclusiveRadiusKilometers, DateTime? startTimeUTC /* null if Start Immediate */,
      DateTime? endTimeUTC /* null if Run Forever */, byte exclusiveMessageID, List<decimal> exclusiveLatitude /* max 5*/,
      List<decimal> exclusiveLongitude /* max 5 */, List<decimal> exclusiveRadius /* max 5 */, byte timeMessageID, bool workSun, bool workMon, bool workTue, bool workWed,
      bool workThur, bool workFri, bool workSat, byte startHourUTC, byte endHourUTC)
    {
      FenceConfig config = new FenceConfig();

      if (inclusiveProductWatch)
      {
        config.inclusive = new FenceConfig.InclusiveConfig();
        config.inclusive.inclusiveMessageID = inclusiveMessageID;
        config.inclusive.InclusiveLatitude = inclusiveLatitude;
        config.inclusive.inclusiveLongitude = inclusiveLongitude;
        config.inclusive.inclusiveRadius = inclusiveRadiusKilometers;
        config.inclusive.startTimeUTC = startTimeUTC;
        config.inclusive.endTimeUTC = endTimeUTC;
      }

      if (exclusiveProductWatch)
      {
        config.exclusive = new FenceConfig.ExclusiveConfig();
        config.exclusive.exclusiveMessageID = exclusiveMessageID;
        config.exclusive.exclusiveLatitude = exclusiveLatitude;
        config.exclusive.exclusiveLongitude = exclusiveLongitude;
        config.exclusive.exclusiveRadius = exclusiveRadius;
      }

      if (timeBasedProductWatch)
      {
        config.timeBased = new FenceConfig.TimeBasedConfig();
        config.timeBased.timeMessageID = timeMessageID;
        config.timeBased.workSun = workSun;
        config.timeBased.workMon = workMon;
        config.timeBased.workTue = workTue;
        config.timeBased.workWed = workWed;
        config.timeBased.workThur = workThur;
        config.timeBased.workFri = workFri;
        config.timeBased.workSat = workSat;
        config.timeBased.startHourUTC = startHourUTC;
        config.timeBased.endHourUTC = endHourUTC;
      }

      uint bitPosition = 0;
      return PlatformMessage.SerializePlatformMessage(config, null, ref bitPosition, false);
    }

    public static byte[] FormatProductWatchActivation(bool? inclusiveWatchActive, bool? exclusiveWatchActive, bool? timeBasedWatchActive)
    {
      ProductWatchActivation product = new ProductWatchActivation();
      product.inclusiveWatchActivate = inclusiveWatchActive;
      product.exclusiveWatchActivate = exclusiveWatchActive;
      product.timeBasedWatchActivate = timeBasedWatchActive;

      uint bitPosition = 0;
      byte[] raw = new byte[5];
      product.Serialize(SerializationAction.Serialize, raw, ref bitPosition);
      return raw;
    }

    public static List<PLReportingIntervalsConfigPayload> FormatReportIntervalsConfig(TimeSpan? eventIntervals, EventFrequency? level1TransmissionFrequency, EventFrequency? level2TransmissionFrequency, EventFrequency? level3TransmissionFrequency,
      TimeSpan? nextMessageInterval, bool? globalGramEnable, DateTime? reportStartTimeUTC, EventFrequency? diagnosticTransmissionFrequency,
      SMUFuelReporting? smuFuelReporting, bool? startStopConfigEnabled, int? positionReportConfig, bool? reportSMU, string moduleCode, DeviceTypeEnum deviceType)
    {
      List<PLReportingIntervalsConfigPayload> payload = new List<PLReportingIntervalsConfigPayload>();

      if (deviceType == DeviceTypeEnum.PL321)
      {
        if (PLOutboundFormatter.IsPL321VIMS(moduleCode, deviceType))
        {
          if (globalGramEnable.HasValue || positionReportConfig.HasValue || reportStartTimeUTC.HasValue || reportSMU.HasValue || (smuFuelReporting.HasValue && smuFuelReporting != SMUFuelReporting.Fuel))
          {
            PL121AndPL321VIMSOTAConfigMessages otaPL121 = new PL121AndPL321VIMSOTAConfigMessages();
            otaPL121.SubType = 0X00;
            otaPL121.GlobalGramEnable = globalGramEnable;
            otaPL121.SetPositionReportConfig(positionReportConfig, DeviceTypeEnum.PL321);
            otaPL121.ReportStartTimeUTC = reportStartTimeUTC;
            otaPL121.ReportSMU = reportSMU;
            //otaPL121.SmuFuelReporting = smuFuelReporting;
            otaPL121.Pl321VIMSSMUReporting = smuFuelReporting;
            uint bitPosition = 0;
            payload.Add(new PLReportingIntervalsConfigPayload(PlatformMessage.SerializePlatformMessage(otaPL121, null, ref bitPosition, false)));
          }
          if (eventIntervals.HasValue || level1TransmissionFrequency.HasValue || level2TransmissionFrequency.HasValue || level3TransmissionFrequency.HasValue ||
            nextMessageInterval.HasValue || diagnosticTransmissionFrequency.HasValue || (smuFuelReporting.HasValue && smuFuelReporting != SMUFuelReporting.SMU) || startStopConfigEnabled.HasValue)
          {
            PL121AndPL321VIMSOTAConfigMessages otaPL321 = new PL121AndPL321VIMSOTAConfigMessages();
            otaPL321.SubType = 0X02;
            otaPL321.EventIntervals = eventIntervals;
            otaPL321.Level1Frequency = level1TransmissionFrequency;
            otaPL321.Level2Frequency = level2TransmissionFrequency;
            otaPL321.Level3Frequency = level3TransmissionFrequency;
            otaPL321.NextMessageInterval = nextMessageInterval;
            otaPL321.DiagnosticTransmissionFrequency = diagnosticTransmissionFrequency;
            //otaPL321.SmuFuelReporting = smuFuelReporting;
            otaPL321.Pl321VIMSFuelReporting = smuFuelReporting;
            otaPL321.StartStopConfigurationEnable = startStopConfigEnabled;            

            uint bitPosition321 = 0;
            payload.Add(new PLReportingIntervalsConfigPayload(PlatformMessage.SerializePlatformMessage(otaPL321, null, ref bitPosition321, false)));
          }
          return payload;
        }
        else
        {
          PL321OTAConfigMessages ota = new PL321OTAConfigMessages();
          ota.EventIntervals = eventIntervals;
          ota.Level1Frequency = level1TransmissionFrequency;
          ota.Level2Frequency = level2TransmissionFrequency;
          ota.Level3Frequency = level3TransmissionFrequency;
          ota.NextMessageInterval = nextMessageInterval;
          ota.GlobalGramEnable = globalGramEnable;
          ota.ReportStartTimeUTC = reportStartTimeUTC;
          ota.DiagnosticTransmissionFrequency = diagnosticTransmissionFrequency;
          ota.SmuFuelReporting = smuFuelReporting;
          ota.StartStopConfigurationEnable = startStopConfigEnabled;
          ota.PositionReportConfig = positionReportConfig;

          uint bitPosition = 0;
          payload.Add(new PLReportingIntervalsConfigPayload(PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false)));
          return payload;
        }
      }
      else
      {
        PL121AndPL321VIMSOTAConfigMessages ota = new PL121AndPL321VIMSOTAConfigMessages();
        ota.SubType = 0X00;
        ota.GlobalGramEnable = globalGramEnable;
        ota.SetPositionReportConfig(positionReportConfig, DeviceTypeEnum.PL121);
        ota.ReportStartTimeUTC = reportStartTimeUTC;
        ota.ReportSMU = reportSMU;

        uint bitPosition = 0;
        payload.Add(new PLReportingIntervalsConfigPayload(PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false)));
        return payload;
      }
    }

    public static byte[] FormatRuntimeAdjustment(TimeSpan newRuntimeValue, string moduleCode, DeviceTypeEnum deviceType)
    {
      if (deviceType == DeviceTypeEnum.PL321)
      {
        if (PLOutboundFormatter.IsPL321VIMS(moduleCode, deviceType))
        {
          PL121AndPL321VIMSOTAConfigMessages ota = new PL121AndPL321VIMSOTAConfigMessages();
          ota.SubType = 0X02;
          ota.RuntimeHoursAdj = newRuntimeValue;
          uint bitPosition = 0;
          return PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);
        }
        else
        {
          PL321OTAConfigMessages ota = new PL321OTAConfigMessages();
          ota.RuntimeHoursAdj = newRuntimeValue;
          uint bitPosition = 0;
          return PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);
        }
      }
      else
      {
        PL121AndPL321VIMSOTAConfigMessages ota = new PL121AndPL321VIMSOTAConfigMessages();
        ota.SubType = 0X00;
        ota.RuntimeHoursAdj = newRuntimeValue;
        uint bitPosition = 0;
        return PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);
      }
    }

    public static byte[] FormatDigitalInputParameters(String gpsDeviceId, InputConfig? input1Config, TimeSpan? input1DelayTime,
      DigitalInputMonitoringConditions? digitalInput1MonitoringCondition, string description1, InputConfig? input2Config, TimeSpan? input2DelayTime,
      DigitalInputMonitoringConditions? digitalInput2MonitoringCondition, string description2, InputConfig? input3Config, TimeSpan? input3DelayTime,
      DigitalInputMonitoringConditions? digitalInput3MonitoringCondition, string description3, InputConfig? input4Config, TimeSpan? input4DelayTime,
      DigitalInputMonitoringConditions? digitalInput4MonitoringCondition, string description4)
    {
      DeviceTypeEnum deviceType = DeviceTypeEnum.PL321;

      if (PLOutboundFormatter.IsPL321VIMS(gpsDeviceId, deviceType))
      {
        PL121AndPL321VIMSOTAConfigMessages ota = new PL121AndPL321VIMSOTAConfigMessages();
        ota.SubType = 0X02;
        ota.InputConfig1 = input1Config;
        ota.Input1DelayTime = input1DelayTime;
        ota.DigitalInput1MonitoringCondition = digitalInput1MonitoringCondition;

        ota.InputConfig2 = input2Config;
        ota.Input2DelayTime = input2DelayTime;
        ota.DigitalInput2MonitoringCondition = digitalInput2MonitoringCondition;

        ota.InputConfig3 = input3Config;
        ota.Input3DelayTime = input3DelayTime;
        ota.DigitalInput3MonitoringCondition = digitalInput3MonitoringCondition;

        ota.InputConfig4 = input4Config;
        ota.Input4DelayTime = input4DelayTime;
        ota.DigitalInput4MonitoringCondition = digitalInput4MonitoringCondition;

        ota.Input1Description = description1;
        ota.Input2Description = description2;
        ota.Input3Description = description3;
        ota.Input4Description = description4;

        uint bitPosition = 0;
        return PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);
      }
      else
      {
        PL321OTAConfigMessages ota = new PL321OTAConfigMessages();
        ota.InputConfig1 = input1Config;
        ota.Input1DelayTime = input1DelayTime;
        ota.DigitalInput1MonitoringCondition = digitalInput1MonitoringCondition;

        ota.InputConfig2 = input2Config;
        ota.Input2DelayTime = input2DelayTime;
        ota.DigitalInput2MonitoringCondition = digitalInput2MonitoringCondition;

        ota.InputConfig3 = input3Config;
        ota.Input3DelayTime = input3DelayTime;
        ota.DigitalInput3MonitoringCondition = digitalInput3MonitoringCondition;

        ota.InputConfig4 = input4Config;
        ota.Input4DelayTime = input4DelayTime;
        ota.DigitalInput4MonitoringCondition = digitalInput4MonitoringCondition;

        ota.Input1Description = description1;
        ota.Input2Description = description2;
        ota.Input3Description = description3;
        ota.Input4Description = description4;

        uint bitPosition = 0;
        return PlatformMessage.SerializePlatformMessage(ota, null, ref bitPosition, false);
      }
    }

    private static bool IsPL321VIMS(string moduleCode, DeviceTypeEnum deviceType)
    {
      using (INH_OP Ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        return (from d in Ctx.DeviceReadOnly 
                       join dp in Ctx.DevicePersonalityReadOnly on d.ID equals dp.fk_DeviceID
                     where d.GpsDeviceID == moduleCode && d.fk_DeviceTypeID == (int) deviceType 
                     && dp.fk_PersonalityTypeID == (int)PersonalityTypeEnum.PL321ModuleType && dp.Value == "PL321VIMS" select d.ID).Any();
        
      }                   
    }
  }
}
