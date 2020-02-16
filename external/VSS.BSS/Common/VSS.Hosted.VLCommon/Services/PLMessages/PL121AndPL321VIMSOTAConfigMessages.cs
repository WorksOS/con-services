using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.PLMessages
{
  public class PL121AndPL321VIMSOTAConfigMessages : PLBaseMessage
  {
    public static new readonly int kPacketID = 0x11;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;
    public byte Version = 0x01;
    public byte SequenceNumber = 0x00;
  

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref Version);
      serializer(action, raw, ref bitPosition, 8, ref SequenceNumber);
      if (SubType == 0x00)
        serializePL121OTA(action, raw, ref bitPosition);
      else if (SubType == 0x02)
        serializePL321VIMSOTA(action, raw, ref bitPosition);      
    }

    private void serializePL121OTA(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (action == SerializationAction.Hydrate)
      {
        HydratePL121OTAReportingMessage(action, raw, ref bitPosition);
      }
      else
      {
        serializePL121OTAMessage(action, raw, ref bitPosition);
      }
    }

    private void serializePL321VIMSOTA(SerializationAction action, byte[] raw, ref uint bitPosition)
    {     
      if (action == SerializationAction.Hydrate)
      {
        HydratePL321VIMSOTAReportingMessage(action, raw, ref bitPosition);
      }
      else
      {
        serializePL321VIMSOTAMessage(action, raw, ref bitPosition);
      }
    }

    private void HydratePL121OTAReportingMessage(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      while ((bitPosition / 8) < raw.Length)
      {
        HydratePL121ReportingField(action, raw, ref bitPosition);
      }
    }

    private void HydratePL121ReportingField(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      byte fieldID = 0;
      serializer(action, raw, ref bitPosition, 8, ref fieldID);

      switch ((FieldID)fieldID)
      {
        case FieldID.PL121PositionReporting:
          positionReportConfigFieldID = (byte)FieldID.PL121PositionReporting;
          serializer(action, raw, ref bitPosition, 8, ref positionReportConfig);
          break;
        case FieldID.SMU:
          smuFieldID = (byte)FieldID.SMU;
          BigEndianSerializer(action, raw, ref bitPosition, 3, ref smu);
          break;
        case FieldID.PL121SMUReporting:
          smuReportingFieldID = (byte)FieldID.PL121SMUReporting;
          serializer(action, raw, ref bitPosition, 8, ref smuReporting);
          break;
        case FieldID.GlobalGramEnable:
          globalGramEnableFieldID = (byte)FieldID.GlobalGramEnable;
          serializer(action, raw, ref bitPosition, 8, ref globalGramEnable);
          break;
        case FieldID.ReportStartTime:
          reportStartTimeFieldID = (byte)FieldID.ReportStartTime;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref reportStartTime);
          break;
      }
    }

    private void HydratePL321VIMSOTAReportingMessage(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      while ((bitPosition / 8) < raw.Length)
      {
        HydratePL321VIMSReportingField(action, raw, ref bitPosition);
      }
    }

    private void HydratePL321VIMSReportingField(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      byte fieldID = 0;
      serializer(action, raw, ref bitPosition, 8, ref fieldID);

      switch ((FieldID)fieldID)
      {
        case FieldID.SMU:
          smuFieldID = (byte)FieldID.SMU;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref smu);
          break;
        case FieldID.Level1TransmissionFrequency:
          level1FrequencyFieldID = (byte)FieldID.Level1TransmissionFrequency;
          string level1 = string.Empty;
          serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level1);
          byte.TryParse(level1, out level1Frequency);
          break;
        case FieldID.Level2TransmissionFrequency:
          level2FrequencyFieldID = (byte)FieldID.Level2TransmissionFrequency;
          string level2 = string.Empty;
          serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level2);
          byte.TryParse(level2, out level2Frequency);
          break;
        case FieldID.Level3TransmissionFrequency:
          level3FrequencyFieldID = (byte)FieldID.Level3TransmissionFrequency;
          string level3 = string.Empty;
          serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level3);
          byte.TryParse(level3, out level3Frequency);
          break;
        case FieldID.DiagnosticTransmissionFrequency:
          diagnosticTransmissionFrequencyFieldID = (byte)FieldID.DiagnosticTransmissionFrequency;
          string diag = string.Empty;
          serializeFixedLengthString(action, raw, ref bitPosition, 1, ref diag);
          byte.TryParse(diag, out diagnosticTransmissionFrequency);
          break;
        case FieldID.SMUFuelReporting:
          smuFuelReportingFieldID = (byte)FieldID.SMUFuelReporting;
          serializer(action, raw, ref bitPosition, 8, ref smuFuelReporting);
          break;
        case FieldID.NextMessageInterval:
          nextMessageIntervalHoursFieldID = (byte)FieldID.NextMessageInterval;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref nextMessageIntervalHours);
          break;
        case FieldID.StartStopConfiguration:
          startStopConfigurationFieldID = (byte)FieldID.StartStopConfiguration;
          serializer(action, raw, ref bitPosition, 8, ref startStopConfiguration);
          break;
        case FieldID.EventInterval:
          eventIntervalHoursFieldID = (byte)FieldID.EventInterval;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref eventIntervalHours);
          break;
        case FieldID.DigitalInput1MonitoringCondition:
          digitalInput1MonitoringConditionFieldID = (byte)FieldID.DigitalInput1MonitoringCondition;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput1MonitoringCondition);
          break;
        case FieldID.DigitalInput2MonitoringCondition:
          digitalInput2MonitoringConditionFieldID = (byte)FieldID.DigitalInput2MonitoringCondition;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput2MonitoringCondition);
          break;
        case FieldID.DigitalInput3MonitoringCondition:
          digitalInput3MonitoringConditionFieldID = (byte)FieldID.DigitalInput3MonitoringCondition;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput3MonitoringCondition);
          break;
        case FieldID.DigitalInput4MonitoringCondition:
          digitalInput4MonitoringConditionFieldID = (byte)FieldID.DigitalInput4MonitoringCondition;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput4MonitoringCondition);
          break;
        case FieldID.DigitalInput1Config:
          inputConfig1FieldID = (byte)FieldID.DigitalInput1Config;
          serializer(action, raw, ref bitPosition, 8, ref inputConfig1);
          break;
        case FieldID.DigitalInput2Config:
          inputConfig2FieldID = (byte)FieldID.DigitalInput2Config;
          serializer(action, raw, ref bitPosition, 8, ref inputConfig2);
          break;
        case FieldID.DigitalInput3Config:
          inputConfig3FieldID = (byte)FieldID.DigitalInput3Config;
          serializer(action, raw, ref bitPosition, 8, ref inputConfig3);
          break;
        case FieldID.DigitalInput4Config:
          inputConfig4FieldID = (byte)FieldID.DigitalInput4Config;
          serializer(action, raw, ref bitPosition, 8, ref inputConfig4);
          break;
        case FieldID.DigitalInput1DelayTime:
          input1DelayTimeFieldID = (byte)FieldID.DigitalInput1DelayTime;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref input1DelayTime);
          break;
        case FieldID.DigitalInput2DelayTime:
          input2DelayTimeFieldID = (byte)FieldID.DigitalInput2DelayTime;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref input2DelayTime);
          break;
        case FieldID.DigitalInput3DelayTime:
          input3DelayTimeFieldID = (byte)FieldID.DigitalInput3DelayTime;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref input3DelayTime);
          break;
        case FieldID.DigitalInput4DelayTime:
          input4DelayTimeFieldID = (byte)FieldID.DigitalInput4DelayTime;
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref input4DelayTime);
          break;
        case FieldID.DigitalInput1Description:
          input1DescriptionFieldID = (byte)FieldID.DigitalInput1Description;
          serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input1Description);
          break;
        case FieldID.DigitalInput2Description:
          input2DescriptionFieldID = (byte)FieldID.DigitalInput2Description;
          serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input2Description);
          break;
        case FieldID.DigitalInput3Description:
          input3DescriptionFieldID = (byte)FieldID.DigitalInput3Description;
          serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input3Description);
          break;
        case FieldID.DigitalInput4Description:
          input4DescriptionFieldID = (byte)FieldID.DigitalInput4Description;
          serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input4Description);
          break;
      }
    }
    private void serializePL121OTAMessage(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      SerializePL121SMUAdj(action, raw, ref bitPosition);
      SerializePL121ReportingIntervals(action, raw, ref bitPosition);
    }

    private void SerializePL121SMUAdj(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (smuFieldID == (byte)FieldID.SMU)
      {
        serializer(action, raw, ref bitPosition, 8, ref smuFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 3, ref smu);
      }
    }

    private void SerializePL121ReportingIntervals(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (globalGramEnableFieldID == (byte)FieldID.GlobalGramEnable)
      {
        serializer(action, raw, ref bitPosition, 8, ref globalGramEnableFieldID);
        serializer(action, raw, ref bitPosition, 8, ref globalGramEnable);
      }
      if (reportStartTimeFieldID == (byte)FieldID.ReportStartTime)
      {
        serializer(action, raw, ref bitPosition, 8, ref reportStartTimeFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref reportStartTime);
      }
      if (positionReportConfigFieldID == (byte)FieldID.PL121PositionReporting)
      {
        serializer(action, raw, ref bitPosition, 8, ref positionReportConfigFieldID);
        serializer(action, raw, ref bitPosition, 8, ref positionReportConfig);
      }
      if (smuReportingFieldID == (byte)FieldID.PL121SMUReporting)
      {
        serializer(action, raw, ref bitPosition, 8, ref smuReportingFieldID);
        serializer(action, raw, ref bitPosition, 8, ref smuReporting);
      }
      if (pl321VIMSSMUReportingID == (byte)FieldID.PL121SMUReporting)
      {
        serializer(action, raw, ref bitPosition, 8, ref pl321VIMSSMUReportingID);
        serializer(action, raw, ref bitPosition, 8, ref pl321VIMSSMUReporting);
      }
    }

    private void serializePL321VIMSOTAMessage(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      SerializePL321VIMSSMUAdj(action, raw, ref bitPosition);
      SerializePL321VIMSReportingIntervals(action, raw, ref bitPosition);
      SerializePL321VIMSDigitalInputs(action, raw, ref bitPosition);
    }

    private void SerializePL321VIMSDigitalInputs(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (digitalInput1MonitoringConditionFieldID == (byte)FieldID.DigitalInput1MonitoringCondition)
      {
        serializer(action, raw, ref bitPosition, 8, ref digitalInput1MonitoringConditionFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput1MonitoringCondition);
      }
      if (digitalInput2MonitoringConditionFieldID == (byte)FieldID.DigitalInput2MonitoringCondition)
      {
        serializer(action, raw, ref bitPosition, 8, ref digitalInput2MonitoringConditionFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput2MonitoringCondition);
      }
      if (digitalInput3MonitoringConditionFieldID == (byte)FieldID.DigitalInput3MonitoringCondition)
      {
        serializer(action, raw, ref bitPosition, 8, ref digitalInput3MonitoringConditionFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput3MonitoringCondition);
      }
      if (digitalInput4MonitoringConditionFieldID == (byte)FieldID.DigitalInput4MonitoringCondition)
      {
        serializer(action, raw, ref bitPosition, 8, ref digitalInput4MonitoringConditionFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref digitalInput4MonitoringCondition);
      }
      if (inputConfig1FieldID == (byte)FieldID.DigitalInput1Config)
      {
        serializer(action, raw, ref bitPosition, 8, ref inputConfig1FieldID);
        serializer(action, raw, ref bitPosition, 8, ref inputConfig1);
      }
      if (inputConfig2FieldID == (byte)FieldID.DigitalInput2Config)
      {
        serializer(action, raw, ref bitPosition, 8, ref inputConfig2FieldID);
        serializer(action, raw, ref bitPosition, 8, ref inputConfig2);
      }
      if (inputConfig3FieldID == (byte)FieldID.DigitalInput3Config)
      {
        serializer(action, raw, ref bitPosition, 8, ref inputConfig3FieldID);
        serializer(action, raw, ref bitPosition, 8, ref inputConfig3);
      }
      if (inputConfig4FieldID == (byte)FieldID.DigitalInput4Config)
      {
        serializer(action, raw, ref bitPosition, 8, ref inputConfig4FieldID);
        serializer(action, raw, ref bitPosition, 8, ref inputConfig4);
      }
      if (input1DelayTimeFieldID == (byte)FieldID.DigitalInput1DelayTime)
      {
        serializer(action, raw, ref bitPosition, 8, ref input1DelayTimeFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref input1DelayTime);
      }
      if (input2DelayTimeFieldID == (byte)FieldID.DigitalInput2DelayTime)
      {
        serializer(action, raw, ref bitPosition, 8, ref input2DelayTimeFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref input2DelayTime);
      }
      if (input3DelayTimeFieldID == (byte)FieldID.DigitalInput3DelayTime)
      {
        serializer(action, raw, ref bitPosition, 8, ref input3DelayTimeFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref input3DelayTime);
      }
      if (input4DelayTimeFieldID == (byte)FieldID.DigitalInput4DelayTime)
      {
        serializer(action, raw, ref bitPosition, 8, ref input4DelayTimeFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref input4DelayTime);
      }
      if (input1DescriptionFieldID == (byte)FieldID.DigitalInput1Description)
      {
        serializer(action, raw, ref bitPosition, 8, ref input1DescriptionFieldID);
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input1Description);
      }
      if (input2DescriptionFieldID == (byte)FieldID.DigitalInput2Description)
      {
        serializer(action, raw, ref bitPosition, 8, ref input2DescriptionFieldID);
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input2Description);
      }
      if (input3DescriptionFieldID == (byte)FieldID.DigitalInput3Description)
      {
        serializer(action, raw, ref bitPosition, 8, ref input3DescriptionFieldID);
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input3Description);
      }
      if (input4DescriptionFieldID == (byte)FieldID.DigitalInput4Description)
      {
        serializer(action, raw, ref bitPosition, 8, ref input4DescriptionFieldID);
        serializeFixedLengthString(action, raw, ref bitPosition, 24, ref input4Description);
      }
    }

    private void SerializePL321VIMSSMUAdj(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if ((FieldID)smuFieldID != FieldID.NotUsed)
      {
        serializer(action, raw, ref bitPosition, 8, ref smuFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref smu);
      }
    }

    private void SerializePL321VIMSReportingIntervals(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (level1FrequencyFieldID == (byte)FieldID.Level1TransmissionFrequency)
      {
        serializer(action, raw, ref bitPosition, 8, ref level1FrequencyFieldID);
        string level1FrequencyStr = level1Frequency.ToNullString();
        serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level1FrequencyStr);

      }
      if (level2FrequencyFieldID == (byte)FieldID.Level2TransmissionFrequency)
      {
        serializer(action, raw, ref bitPosition, 8, ref level2FrequencyFieldID);
        string level2FrequencyStr = level2Frequency.ToNullString();
        serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level2FrequencyStr);

      }
      if (level3FrequencyFieldID == (byte)FieldID.Level3TransmissionFrequency)
      {
        serializer(action, raw, ref bitPosition, 8, ref level3FrequencyFieldID);
        string level3FrequencyStr = level3Frequency.ToNullString();
        serializeFixedLengthString(action, raw, ref bitPosition, 1, ref level3FrequencyStr);

      }
      if (diagnosticTransmissionFrequencyFieldID == (byte)FieldID.DiagnosticTransmissionFrequency)
      {
        serializer(action, raw, ref bitPosition, 8, ref diagnosticTransmissionFrequencyFieldID);
        string diagnosticTransmissionFrequencyStr = diagnosticTransmissionFrequency.ToNullString();
        serializeFixedLengthString(action, raw, ref bitPosition, 1, ref diagnosticTransmissionFrequencyStr);

      }

      //if (smuFuelReportingFieldID == (byte)FieldID.SMUFuelReporting)
      //{
      //  serializer(action, raw, ref bitPosition, 8, ref smuFuelReportingFieldID);
      //  serializer(action, raw, ref bitPosition, 8, ref smuFuelReporting);
      //}

      if (pl321VIMSFuelReportingID == (byte)FieldID.PL321VIMSGatewaySMUFuelReporting)
      {
        serializer(action, raw, ref bitPosition, 8, ref pl321VIMSFuelReportingID);
        serializer(action, raw, ref bitPosition, 8, ref pl321VIMSFuelReporting);

      }
      if (nextMessageIntervalHoursFieldID == (byte)FieldID.NextMessageInterval)
      {
        serializer(action, raw, ref bitPosition, 8, ref nextMessageIntervalHoursFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref nextMessageIntervalHours);

      }
      if (startStopConfigurationFieldID == (byte)FieldID.StartStopConfiguration)
      {
        serializer(action, raw, ref bitPosition, 8, ref startStopConfigurationFieldID);
        serializer(action, raw, ref bitPosition, 8, ref startStopConfiguration);

      }
      if (eventIntervalHoursFieldID == (byte)FieldID.EventInterval)
      {
        serializer(action, raw, ref bitPosition, 8, ref eventIntervalHoursFieldID);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref eventIntervalHours);

      }
    }


    public List<PLConfigData.PLConfigBase> GetPLConfigData(DateTime sentUTC)
    {
      List<PLConfigData.PLConfigBase> configData = new List<PLConfigData.PLConfigBase>();
      PLConfigData.GeneralRegistry general = GetGeneralRegistryFields(sentUTC);
      PLConfigData.TransmissionRegistry transmission = GetTransmissionRegistryFields(sentUTC);
      PLConfigData.DigitalRegistry digital = GetDigitalRegistryFields(sentUTC);

      if (general != null) configData.Add(general);
      if (transmission != null) configData.Add(transmission);
      if (digital != null) configData.Add(digital);

      if (configData.Count > 0)
        return configData;
      else
        return null;
    }

    private PLConfigData.GeneralRegistry GetGeneralRegistryFields(DateTime sentUTC)
    {
      PLConfigData.GeneralRegistry general = new PLConfigData.GeneralRegistry();

      general.RunTimeHoursAdj = RuntimeHoursAdj.HasValue ? RuntimeHoursAdj.Value : (TimeSpan?)null;
      if (general.RunTimeHoursAdj.HasValue)
        general.RuntimeHoursSentUTC = sentUTC;
      
      general.GlobalGramEnable = GlobalGramEnable.HasValue ? GlobalGramEnable.Value : (bool?)null;
      if (general.GlobalGramEnable.HasValue)
        general.GlobalGramSentUTC = sentUTC;
      
      if (ReportStartTimeUTC.HasValue)
      {
        if(general.ReportSchedule == null)
          general.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule();
        general.ReportSchedule.ReportStartTime = new TimeSpan(ReportStartTimeUTC.Value.Hour, ReportStartTimeUTC.Value.Minute, 0);
        general.ReportSchedule.ReportStartTimeSentUTC = sentUTC;
      }
      
      if (PositionReportConfig.HasValue)
      {
        if(general.ReportSchedule == null)
          general.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule();
        general.ReportSchedule.Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>();
        general.ReportSchedule.Reports.Add(new PLConfigData.GeneralRegistry.ReportingSchedule.Report() { ReportType = "Position", frequency = PositionReportConfig, SentUTC = sentUTC });
      }

      if (ReportSMU.HasValue)
      {
        if (general.ReportSchedule == null)
          general.ReportSchedule = new PLConfigData.GeneralRegistry.ReportingSchedule();
        general.ReportSchedule.Reports = new List<PLConfigData.GeneralRegistry.ReportingSchedule.Report>();
        general.ReportSchedule.Reports.Add(new PLConfigData.GeneralRegistry.ReportingSchedule.Report() { ReportType = "SMU", frequency = ReportSMU.Value ? 1 : 0, SentUTC = sentUTC });
      }      
     
      general.StartStopEnable = StartStopConfigurationEnable.HasValue ? StartStopConfigurationEnable.Value : (bool?)null;
      if (general.StartStopEnable.HasValue)
        general.StartStopEnableSentUTC = sentUTC;

      if (general.RunTimeHoursAdj.HasValue 
        || general.GlobalGramEnable.HasValue 
        || (general.ReportSchedule != null && general.ReportSchedule.ReportStartTime.HasValue)
        || (general.ReportSchedule != null && general.ReportSchedule.Reports != null) 
        || general.StartStopEnable.HasValue)
        return general;
      else
      return null;
    }

    private PLConfigData.DigitalRegistry GetDigitalRegistryFields(DateTime sentUTC)
    {
      PLConfigData.DigitalRegistry digital = new PLConfigData.DigitalRegistry();
      digital.Sensors = new List<PLConfigData.DigitalRegistry.SensorInformation>();
      PLConfigData.DigitalRegistry.SensorInformation sensor1 = new PLConfigData.DigitalRegistry.SensorInformation();
      sensor1.SensorNumber = 1;
      sensor1.DelayTime = Input1DelayTime.HasValue ? Input1DelayTime.Value : (TimeSpan?)null;
      if (sensor1.DelayTime.HasValue)
        sensor1.DelayTimeSentUTC = sentUTC;
      sensor1.Description = Input1Description;
      if (!string.IsNullOrEmpty(sensor1.Description))
        sensor1.DescriptionSentUTC = sentUTC;

      sensor1.MonitorCondition = DigitalInput1MonitoringCondition.HasValue ? DigitalInput1MonitoringCondition : (DigitalInputMonitoringConditions?)null;
      if (sensor1.MonitorCondition.HasValue)
        sensor1.MonitorConditionSentUTC = sentUTC;

      sensor1.SensorConfiguration = InputConfig1.HasValue ? InputConfig1.Value : (InputConfig?)null;
      if (sensor1.SensorConfiguration.HasValue)
        sensor1.SensorConfigSentUTC = sentUTC;

      if (sensor1.DelayTime.HasValue || !string.IsNullOrEmpty(sensor1.Description) || sensor1.MonitorCondition.HasValue ||
      sensor1.SensorConfiguration.HasValue)
        digital.Sensors.Add(sensor1);

      PLConfigData.DigitalRegistry.SensorInformation sensor2 = new PLConfigData.DigitalRegistry.SensorInformation();
      sensor2.SensorNumber = 2;

      sensor2.DelayTime = Input2DelayTime.HasValue ? Input2DelayTime.Value : (TimeSpan?)null;
      if (sensor2.DelayTime.HasValue)
        sensor2.DelayTimeSentUTC = sentUTC;

      sensor2.Description = Input2Description;
      if (!string.IsNullOrEmpty(sensor2.Description))
        sensor2.DescriptionSentUTC = sentUTC;

      sensor2.MonitorCondition = DigitalInput2MonitoringCondition.HasValue ? DigitalInput2MonitoringCondition : (DigitalInputMonitoringConditions?)null;
      if (sensor2.MonitorCondition.HasValue)
        sensor2.MonitorConditionSentUTC = sentUTC;

      sensor2.SensorConfiguration = InputConfig2.HasValue ? InputConfig2.Value : (InputConfig?)null;
      if (sensor2.SensorConfiguration.HasValue)
        sensor2.SensorConfigSentUTC = sentUTC;

      if (sensor2.DelayTime.HasValue || !string.IsNullOrEmpty(sensor2.Description) || sensor2.MonitorCondition.HasValue ||
      sensor2.SensorConfiguration.HasValue)
        digital.Sensors.Add(sensor2);

      PLConfigData.DigitalRegistry.SensorInformation sensor3 = new PLConfigData.DigitalRegistry.SensorInformation();
      sensor3.SensorNumber = 3;

      sensor3.DelayTime = Input3DelayTime.HasValue ? Input3DelayTime.Value : (TimeSpan?)null;
      if (sensor3.DelayTime.HasValue)
        sensor3.DelayTimeSentUTC = sentUTC;

      sensor3.Description = Input3Description;
      if (!string.IsNullOrEmpty(sensor3.Description))
        sensor3.DescriptionSentUTC = sentUTC;

      sensor3.MonitorCondition = DigitalInput3MonitoringCondition.HasValue ? DigitalInput3MonitoringCondition.Value : (DigitalInputMonitoringConditions?)null;
      if (sensor3.MonitorCondition.HasValue)
        sensor3.MonitorConditionSentUTC = sentUTC;

      sensor3.SensorConfiguration = InputConfig3.HasValue ? InputConfig3.Value : (InputConfig?)null;
      if (sensor3.SensorConfiguration.HasValue)
        sensor3.SensorConfigSentUTC = sentUTC;

      if (sensor3.DelayTime.HasValue || !string.IsNullOrEmpty(sensor3.Description) || sensor3.MonitorCondition.HasValue ||
      sensor3.SensorConfiguration.HasValue)
        digital.Sensors.Add(sensor3);

      PLConfigData.DigitalRegistry.SensorInformation sensor4 = new PLConfigData.DigitalRegistry.SensorInformation();
      sensor4.SensorNumber = 4;

      sensor4.DelayTime = Input4DelayTime.HasValue ? Input4DelayTime.Value : (TimeSpan?)null;
      if (sensor4.DelayTime.HasValue)
        sensor4.DelayTimeSentUTC = sentUTC;

      sensor4.Description = Input4Description;
      if (!string.IsNullOrEmpty(sensor4.Description))
        sensor4.DescriptionSentUTC = sentUTC;

      sensor4.MonitorCondition = DigitalInput4MonitoringCondition.HasValue ? DigitalInput4MonitoringCondition.Value : (DigitalInputMonitoringConditions?)null;
      if (sensor4.MonitorCondition.HasValue)
        sensor4.MonitorConditionSentUTC = sentUTC;

      sensor4.SensorConfiguration = InputConfig4.HasValue ? InputConfig4.Value : (InputConfig?)null;
      if (sensor4.SensorConfiguration.HasValue)
        sensor4.SensorConfigSentUTC = sentUTC;

      if (sensor4.DelayTime.HasValue || !string.IsNullOrEmpty(sensor4.Description) || sensor4.MonitorCondition.HasValue ||
      sensor4.SensorConfiguration.HasValue)
        digital.Sensors.Add(sensor4);

      if (digital.Sensors.Count > 0)
        return digital;
      else
        return null;
    }

    private PLConfigData.TransmissionRegistry GetTransmissionRegistryFields(DateTime sentUTC)
    {
      PLConfigData.TransmissionRegistry transmission = new PLConfigData.TransmissionRegistry();
      transmission.EventIntervalHours = EventIntervals.HasValue ? (int)EventIntervals.Value.TotalHours : (int?)null;
      if (transmission.EventIntervalHours.HasValue)
        transmission.EventIntervalHoursSentUTC = sentUTC;
      transmission.EventReporting = new PLConfigData.TransmissionRegistry.EventReportingFrequency();

      transmission.EventReporting.DiagnosticFreqCode = DiagnosticTransmissionFrequency.HasValue ? DiagnosticTransmissionFrequency.Value : (EventFrequency?)null;
      if (transmission.EventReporting.DiagnosticFreqCode.HasValue)
        transmission.EventReporting.DiagnosticFreqSentUTC = sentUTC;

      transmission.EventReporting.Level1EventFreqCode = Level1Frequency.HasValue ? Level1Frequency.Value : (EventFrequency?)null;
      if (transmission.EventReporting.Level1EventFreqCode.HasValue)
        transmission.EventReporting.Level1EventFreqSentUTC = sentUTC;

      transmission.EventReporting.Level2EventFreqCode = Level2Frequency.HasValue ? Level2Frequency.Value : (EventFrequency?)null;
      if (transmission.EventReporting.Level2EventFreqCode.HasValue)
        transmission.EventReporting.Level2EventFreqSentUTC = sentUTC;

      transmission.EventReporting.Level3EventFreqCode = Level3Frequency.HasValue ? Level3Frequency.Value : (EventFrequency?)null;
      if (transmission.EventReporting.Level3EventFreqCode.HasValue)
        transmission.EventReporting.Level3EventFreqSentUTC = sentUTC;

      transmission.NextMessageInterval = NextMessageInterval.HasValue ? (int)NextMessageInterval.Value.TotalHours : (int?)null;
      if (transmission.NextMessageInterval.HasValue)
        transmission.NextMessageIntervalSentUTC = sentUTC;

      transmission.SMUFuel = SmuFuelReporting.HasValue ? SmuFuelReporting.Value : (SMUFuelReporting?)null;
      if (transmission.SMUFuel.HasValue)
        transmission.SMUFuelSentUTC = sentUTC;

      if (transmission.EventIntervalHours.HasValue || transmission.EventReporting.DiagnosticFreqCode.HasValue ||
        transmission.EventReporting.Level1EventFreqCode.HasValue || transmission.EventReporting.Level2EventFreqCode.HasValue ||
        transmission.EventReporting.Level3EventFreqCode.HasValue || transmission.NextMessageInterval.HasValue ||
        transmission.SMUFuel.HasValue)
        return transmission;
      else
        return null;
    }

    #region SMU Adjustment Members

    public TimeSpan? RuntimeHoursAdj
    {
      get
      {
        if (((FieldID)smuFieldID) == FieldID.NotUsed)
          return null;
        else return TimeSpan.FromMinutes(smu);
      }
      set
      {
        if (value.HasValue)
        {
          smuFieldID = (byte)FieldID.SMU;
          smu = (uint)value.Value.TotalMinutes;
        }
        else
        {
          smuFieldID = (byte)FieldID.NotUsed;
        }
      }
    }

    private byte smuFieldID = (byte)FieldID.NotUsed;
    private uint smu;

    #endregion

    #region Reporting Intervals Members

    public EventFrequency? Level1Frequency
    {
      get
      {
        if (level1FrequencyFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return (EventFrequency)level1Frequency;
      }
      set
      {
        if (value.HasValue)
        {
          level1FrequencyFieldID = (byte)FieldID.Level1TransmissionFrequency;
          level1Frequency = (byte)value.Value;
        }
        else
        {
          level1FrequencyFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte level1FrequencyFieldID = (byte)FieldID.NotUsed;
    private byte level1Frequency;

    public EventFrequency? Level2Frequency
    {
      get
      {
        if (level2FrequencyFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return (EventFrequency)level2Frequency;
      }
      set
      {
        if (value.HasValue)
        {
          level2FrequencyFieldID = (byte)FieldID.Level2TransmissionFrequency;
          level2Frequency = (byte)value.Value;
        }
        else
        {
          level2FrequencyFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte level2FrequencyFieldID = (byte)FieldID.NotUsed;
    private byte level2Frequency;

    public EventFrequency? Level3Frequency
    {
      get
      {
        if (level3FrequencyFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return (EventFrequency)level3Frequency;
      }
      set
      {
        if (value.HasValue)
        {
          level3FrequencyFieldID = (byte)FieldID.Level3TransmissionFrequency;
          level3Frequency = (byte)value.Value;
        }
        else
        {
          level3FrequencyFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte level3FrequencyFieldID = (byte)FieldID.NotUsed;
    private byte level3Frequency;
    public EventFrequency? DiagnosticTransmissionFrequency
    {
      get
      {
        if (diagnosticTransmissionFrequencyFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return (EventFrequency)diagnosticTransmissionFrequency;
      }
      set
      {
        if (value.HasValue)
        {
          diagnosticTransmissionFrequencyFieldID = (byte)FieldID.DiagnosticTransmissionFrequency;
          diagnosticTransmissionFrequency = (byte)value.Value;
        }
        else
        {
          diagnosticTransmissionFrequencyFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte diagnosticTransmissionFrequencyFieldID = (byte)FieldID.NotUsed;
    private byte diagnosticTransmissionFrequency;
    public SMUFuelReporting? SmuFuelReporting
    {
      get
      {
        if (smuFuelReportingFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return (SMUFuelReporting)smuFuelReporting;
      }
      set
      {
        if (value.HasValue)
        {
          smuFuelReportingFieldID = (byte)FieldID.SMUFuelReporting;
          smuFuelReporting = (byte)value.Value;
        }
        else
        {
          smuFuelReportingFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte smuFuelReporting;
    private byte smuFuelReportingFieldID = (byte)FieldID.NotUsed;
    public int? PositionReportConfig
    {
      get
      {
        if (positionReportConfigFieldID == (byte)FieldID.NotUsed)
          return null;
        else return positionReportConfig;
      }
    }
    public void SetPositionReportConfig(int? value, DeviceTypeEnum deviceType)
    {
      if (value.HasValue)
      {
        positionReportConfig = value.Value;
        if (deviceType == DeviceTypeEnum.PL121 || deviceType == DeviceTypeEnum.PL321)
        {
          positionReportConfigFieldID = (byte)FieldID.PL121PositionReporting;
        }
        else
        {
          throw new NotSupportedException("Unsupported device type: " + deviceType);
        }
      }
      else
      {
        positionReportConfig = 0;
        positionReportConfigFieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte positionReportConfigFieldID = (byte)FieldID.NotUsed;
    private int positionReportConfig;
    public TimeSpan? NextMessageInterval
    {
      get
      {
        if (nextMessageIntervalHoursFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return TimeSpan.FromHours((double)nextMessageIntervalHours);
      }
      set
      {
        if (value.HasValue)
        {
          nextMessageIntervalHoursFieldID = (byte)FieldID.NextMessageInterval;
          nextMessageIntervalHours = (ushort)value.Value.TotalHours;
        }
        else nextMessageIntervalHoursFieldID = (byte)FieldID.NotUsed;
      }
    }
    private ushort nextMessageIntervalHours;
    private byte nextMessageIntervalHoursFieldID = (byte)FieldID.NotUsed;
    public bool? GlobalGramEnable
    {
      get
      {
        if (globalGramEnableFieldID == (byte)FieldID.GlobalGramEnable)
          return globalGramEnable == 0x0C;
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          globalGramEnable = value.Value ? (byte)0x0C : (byte)0x0D;
          globalGramEnableFieldID = (byte)FieldID.GlobalGramEnable;
        }
        else globalGramEnableFieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte globalGramEnableFieldID;
    private byte globalGramEnable;
    public DateTime? ReportStartTimeUTC
    {
      get
      {
        if (reportStartTimeFieldID == (byte)FieldID.ReportStartTime)
          return epoch.AddSeconds(reportStartTime);
        else
          return null;
      }
      set
      {
        if (value.HasValue)
        {
          reportStartTimeFieldID = (byte)FieldID.ReportStartTime;
          reportStartTime = (uint)value.Value.Subtract(epoch).TotalSeconds;
        }
        else
          reportStartTimeFieldID = (byte)FieldID.NotUsed;
      }
    }
    private uint reportStartTime;
    private byte reportStartTimeFieldID;
    public bool? StartStopConfigurationEnable
    {
      get
      {
        if (startStopConfigurationFieldID == (byte)FieldID.StartStopConfiguration)
          return startStopConfiguration == 0x0C;
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          startStopConfigurationFieldID = (byte)FieldID.StartStopConfiguration;
          startStopConfiguration = value.Value ? (byte)0x0C : (byte)0x0D;
        }
        else startStopConfigurationFieldID = (Byte)FieldID.NotUsed;
      }
    }
    private byte startStopConfiguration;
    private byte startStopConfigurationFieldID;

    public TimeSpan? EventIntervals
    {
      get
      {
        if (eventIntervalHoursFieldID == (byte)FieldID.EventInterval)
          return TimeSpan.FromHours((double)eventIntervalHours);
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          eventIntervalHoursFieldID = (byte)FieldID.EventInterval;
          eventIntervalHours = (ushort)value.Value.TotalHours;
        }
        else
          eventIntervalHoursFieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte eventIntervalHoursFieldID;
    private ushort eventIntervalHours;

    public bool isReportingParsed = false;
    public bool isSMUParsed = false;


    public SMUFuelReporting? Pl321VIMSSMUReporting
    {
      get
      {
        if (pl321VIMSSMUReportingID == (byte)FieldID.NotUsed)
          return null;
        else
          return (SMUFuelReporting)pl321VIMSSMUReportingID;
      }
      set
      {
        if (value.HasValue && (value.Value == SMUFuelReporting.SMU || value.Value == SMUFuelReporting.SMUFUEL))
        {
          pl321VIMSSMUReportingID = (byte)FieldID.PL121SMUReporting;
          pl321VIMSSMUReporting = (byte)PL321VIMSSMUFuelStatus.Enable;
        }
        else if (value.HasValue && value.Value == SMUFuelReporting.Off)
        {
          pl321VIMSSMUReportingID = (byte)FieldID.PL121SMUReporting;
          pl321VIMSSMUReporting = (byte)PL321VIMSSMUFuelStatus.Disable;
        }
        else
        {
          pl321VIMSSMUReportingID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte pl321VIMSSMUReporting;
    private byte pl321VIMSSMUReportingID = (byte)FieldID.NotUsed;

    public SMUFuelReporting? Pl321VIMSFuelReporting
    {
      get
      {
        if (pl321VIMSFuelReportingID == (byte)FieldID.NotUsed)
          return null;
        else
          return (SMUFuelReporting)pl321VIMSFuelReportingID;
      }
      set
      {
        if (value.HasValue && (value.Value == SMUFuelReporting.Fuel || value.Value == SMUFuelReporting.SMUFUEL))
        {
          pl321VIMSFuelReportingID = (byte)FieldID.PL321VIMSGatewaySMUFuelReporting;
          pl321VIMSFuelReporting = (byte)PL321VIMSSMUFuelStatus.Enable;
        }
        else if (value.HasValue && value.Value == SMUFuelReporting.Off)
        {
          pl321VIMSFuelReportingID = (byte)FieldID.PL321VIMSGatewaySMUFuelReporting;
          pl321VIMSFuelReporting = (byte)PL321VIMSSMUFuelStatus.Disable;
        }
        else
        {
          pl321VIMSFuelReportingID = (byte)FieldID.NotUsed;
        }
      }
    }
    private byte pl321VIMSFuelReporting;
    private byte pl321VIMSFuelReportingID = (byte)FieldID.NotUsed;


    #endregion

    #region Digital Input Reporting Members

    public DigitalInputMonitoringConditions? DigitalInput1MonitoringCondition
    {
      get
      {
        if (digitalInput1MonitoringConditionFieldID == (byte)FieldID.DigitalInput1MonitoringCondition)
          return (DigitalInputMonitoringConditions)digitalInput1MonitoringCondition;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          digitalInput1MonitoringConditionFieldID = (byte)FieldID.DigitalInput1MonitoringCondition;
          digitalInput1MonitoringCondition = (ushort)value.Value;
        }
      }
    }
    private byte digitalInput1MonitoringConditionFieldID;
    private ushort digitalInput1MonitoringCondition;
    public DigitalInputMonitoringConditions? DigitalInput2MonitoringCondition
    {
      get
      {
        if (digitalInput2MonitoringConditionFieldID == (byte)FieldID.DigitalInput2MonitoringCondition)
          return (DigitalInputMonitoringConditions)digitalInput2MonitoringCondition;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          digitalInput2MonitoringConditionFieldID = (byte)FieldID.DigitalInput2MonitoringCondition;
          digitalInput2MonitoringCondition = (ushort)value.Value;
        }
      }
    }
    private byte digitalInput2MonitoringConditionFieldID;
    private ushort digitalInput2MonitoringCondition;
    public DigitalInputMonitoringConditions? DigitalInput3MonitoringCondition
    {
      get
      {
        if (digitalInput3MonitoringConditionFieldID == (byte)FieldID.DigitalInput3MonitoringCondition)
          return (DigitalInputMonitoringConditions)digitalInput3MonitoringCondition;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          digitalInput3MonitoringConditionFieldID = (byte)FieldID.DigitalInput3MonitoringCondition;
          digitalInput3MonitoringCondition = (ushort)value.Value;
        }
      }
    }
    private byte digitalInput3MonitoringConditionFieldID;
    private ushort digitalInput3MonitoringCondition;
    public DigitalInputMonitoringConditions? DigitalInput4MonitoringCondition
    {
      get
      {
        if (digitalInput4MonitoringConditionFieldID == (byte)FieldID.DigitalInput4MonitoringCondition)
          return (DigitalInputMonitoringConditions)digitalInput4MonitoringCondition;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          digitalInput4MonitoringConditionFieldID = (byte)FieldID.DigitalInput4MonitoringCondition;
          digitalInput4MonitoringCondition = (ushort)value.Value;
        }
      }
    }
    private byte digitalInput4MonitoringConditionFieldID;
    private ushort digitalInput4MonitoringCondition;
    public InputConfig? InputConfig1
    {
      get
      {
        if (inputConfig1FieldID == (byte)FieldID.DigitalInput1Config)
          return (InputConfig)inputConfig1;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          inputConfig1FieldID = (byte)FieldID.DigitalInput1Config;
          inputConfig1 = (byte)value.Value;
        }
        else inputConfig1FieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte inputConfig1FieldID;
    private byte inputConfig1;
    public InputConfig? InputConfig2
    {
      get
      {
        if (inputConfig2FieldID == (byte)FieldID.DigitalInput2Config)
          return (InputConfig)inputConfig2;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          inputConfig2FieldID = (byte)FieldID.DigitalInput2Config;
          inputConfig2 = (byte)value.Value;
        }
        else inputConfig2FieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte inputConfig2FieldID;
    private byte inputConfig2;
    public InputConfig? InputConfig3
    {
      get
      {
        if (inputConfig3FieldID == (byte)FieldID.DigitalInput3Config)
          return (InputConfig)inputConfig3;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          inputConfig3FieldID = (byte)FieldID.DigitalInput3Config;
          inputConfig3 = (byte)value.Value;
        }
        else inputConfig3FieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte inputConfig3FieldID;
    private byte inputConfig3;
    public InputConfig? InputConfig4
    {
      get
      {
        if (inputConfig4FieldID == (byte)FieldID.DigitalInput4Config)
          return (InputConfig)inputConfig4;
        else return null;
      }
      set
      {
        if (value.HasValue)
        {
          inputConfig4FieldID = (byte)FieldID.DigitalInput4Config;
          inputConfig4 = (byte)value.Value;
        }
        else inputConfig4FieldID = (byte)FieldID.NotUsed;
      }
    }
    private byte inputConfig4FieldID;
    private byte inputConfig4;
    public TimeSpan? Input1DelayTime
    {
      get
      {
        if (input1DelayTimeFieldID == (byte)FieldID.DigitalInput1DelayTime)
          return TimeSpan.FromMilliseconds(input1DelayTime * 100.0);
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          input1DelayTimeFieldID = (byte)FieldID.DigitalInput1DelayTime;
          input1DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        }
        else
          input1DelayTimeFieldID = (byte)FieldID.NotUsed;
      }
    }
    private ushort input1DelayTime;
    private byte input1DelayTimeFieldID;

    public TimeSpan? Input2DelayTime
    {
      get
      {
        if (input2DelayTimeFieldID == (byte)FieldID.DigitalInput2DelayTime)
          return TimeSpan.FromMilliseconds(input2DelayTime * 100.0);
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          input2DelayTimeFieldID = (byte)FieldID.DigitalInput2DelayTime;
          input2DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        }
        else
          input2DelayTimeFieldID = (byte)FieldID.NotUsed;
      }
    }
    private ushort input2DelayTime;
    private byte input2DelayTimeFieldID;
    public TimeSpan? Input3DelayTime
    {
      get
      {
        if (input3DelayTimeFieldID == (byte)FieldID.DigitalInput3DelayTime)
          return TimeSpan.FromMilliseconds(input3DelayTime * 100.0);
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          input3DelayTimeFieldID = (byte)FieldID.DigitalInput3DelayTime;
          input3DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        }
        else
          input3DelayTimeFieldID = (byte)FieldID.NotUsed;
      }
    }
    private ushort input3DelayTime;
    private byte input3DelayTimeFieldID;
    public TimeSpan? Input4DelayTime
    {
      get
      {
        if (input4DelayTimeFieldID == (byte)FieldID.DigitalInput4DelayTime)
          return TimeSpan.FromMilliseconds(input4DelayTime * 100.0);
        return null;
      }
      set
      {
        if (value.HasValue)
        {
          input4DelayTimeFieldID = (byte)FieldID.DigitalInput4DelayTime;
          input4DelayTime = (ushort)(value.Value.TotalMilliseconds / 100.0);
        }
        else
          input4DelayTimeFieldID = (byte)FieldID.NotUsed;
      }
    }
    private ushort input4DelayTime;
    private byte input4DelayTimeFieldID;

    public string Input1Description
    {
      get
      {
        if (input1DescriptionFieldID == (byte)FieldID.DigitalInput1Description)
          return input1Description;
        return null;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          input1DescriptionFieldID = (byte)FieldID.DigitalInput1Description;
          input1Description = value.Substring(0, Math.Min(value.Length, 24));
        }
        else
          input1DescriptionFieldID = (byte)FieldID.DigitalInput4DelayTime;
      }
    }
    private string input1Description;
    private byte input1DescriptionFieldID;
    public string Input2Description
    {
      get
      {
        if (input2DescriptionFieldID == (byte)FieldID.DigitalInput2Description)
          return input2Description;
        return null;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          input2DescriptionFieldID = (byte)FieldID.DigitalInput2Description;
          input2Description = value.Substring(0, Math.Min(value.Length, 24));
        }
        else
          input2DescriptionFieldID = (byte)FieldID.NotUsed;
      }
    }
    private string input2Description;
    private byte input2DescriptionFieldID;
    public string Input3Description
    {
      get
      {
        if (input3DescriptionFieldID == (byte)FieldID.DigitalInput3Description)
          return input3Description;
        return null;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          input3DescriptionFieldID = (byte)FieldID.DigitalInput3Description;
          input3Description = value.Substring(0, Math.Min(value.Length, 24));
        }
        else
          input3DescriptionFieldID = (byte)FieldID.NotUsed;
      }
    }
    private string input3Description;
    private byte input3DescriptionFieldID;
    public string Input4Description
    {
      get
      {
        if (input4DescriptionFieldID == (byte)FieldID.DigitalInput4Description)
          return input4Description;
        return null;
      }
      set
      {
        if (!string.IsNullOrEmpty(value))
        {
          input4DescriptionFieldID = (byte)FieldID.DigitalInput4Description;
          input4Description = value.Substring(0, Math.Min(value.Length, 24));
        }
        else
          input4DescriptionFieldID = (byte)FieldID.NotUsed;
      }
    }
    private string input4Description;
    private byte input4DescriptionFieldID;

    #endregion

    public bool? ReportSMU
    {
      get
      {
        if (smuReportingFieldID == (byte)FieldID.NotUsed)
          return null;
        else
          return smuReporting;
      }
      set
      {
        if (value.HasValue)
        {
          smuReportingFieldID = (byte)FieldID.PL121SMUReporting;
          smuReporting = value.Value;
        }
        else
        {
          smuReportingFieldID = (byte)FieldID.NotUsed;
        }
      }
    }
    private bool smuReporting;
    private byte smuReportingFieldID = (byte)FieldID.NotUsed;
  }
}
