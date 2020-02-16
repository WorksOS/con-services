using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon.PLMessages;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  public interface IPLOutboundAPI
  {
      bool SendQueryCommand(INH_OP opCtx1, string moduleCode, PLQueryCommandEnum command);
    bool SendGeoFenceConfig(string moduleCode, bool inclusiveProductWatch,
      decimal inclusiveLatitude, decimal inclusiveLongitude, decimal inclusiveRadiusKilometers, 
      List<decimal> exclusiveLatitude, List<decimal> exclusiveLongitude, List<decimal> exclusiveRadius);
    bool SendDefaultTimeFenceConfig(string moduleCode);
    bool Send24by7TimeFenceConfig(string moduleCode);
    bool SendProductWatchActivation(INH_OP opCtx1, string moduleCode, bool? inclusiveWatchActive, bool? exclusiveWatchActive, bool? timeBasedWatchActive);
    bool SendReportIntervalsConfig(INH_OP opCtx1, string moduleCode, DeviceTypeEnum deviceType, TimeSpan? eventIntervals, EventFrequency? level1TransmissionFrequency, EventFrequency? level2TransmissionFrequency, EventFrequency? level3TransmissionFrequency,
      TimeSpan? nextMessageInterval, bool? globalGramEnable, DateTime? reportStartTimeUTC, EventFrequency? diagnosticTransmissionFrequency,
      SMUFuelReporting? smuFuelReporting, bool? startStopConfigEnabled, int? positionReportConfig);
    bool SendRuntimeAdjustmentConfig(string moduleCode, DeviceTypeEnum deviceType, TimeSpan newRuntimeValue);
    bool SendDigitalInputConfig(INH_OP opCtx1, string moduleCode, InputConfig? input1Config, TimeSpan? input1DelayTime,
      DigitalInputMonitoringConditions? digitalInput1MonitoringCondition, string description1, InputConfig? input2Config, TimeSpan? input2DelayTime,
      DigitalInputMonitoringConditions? digitalInput2MonitoringCondition, string description2, InputConfig? input3Config, TimeSpan? input3DelayTime,
      DigitalInputMonitoringConditions? digitalInput3MonitoringCondition, string description3, InputConfig? input4Config, TimeSpan? input4DelayTime,
      DigitalInputMonitoringConditions? digitalInput4MonitoringCondition, string description4);
  }
}
