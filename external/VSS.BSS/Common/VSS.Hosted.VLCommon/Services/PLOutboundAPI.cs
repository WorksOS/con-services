using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon.PLMessages;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;

//test case review with Shaun and Josh tomorrow
namespace VSS.Hosted.VLCommon
{
  internal class PLOutboundAPI : IPLOutboundAPI
  {
     public bool SendQueryCommand(INH_OP opCtx1, string moduleCode, PLQueryCommandEnum command)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      string subject = "[COMMAND:ACK]";

      string body = string.Empty;
      switch (command)
      {
        case PLQueryCommandEnum.PositionReportQuery:
          body = "Q1";
          break;
        case PLQueryCommandEnum.SMUReportQuery:
          body = "Q2";
          break;
        case PLQueryCommandEnum.StatusQuery:
          body = "Q3";
          break;
        case PLQueryCommandEnum.EventDiagnosticQuery:
          body = "Q4";
          break;
        case PLQueryCommandEnum.FuelReportQuery:
          body = "Q5";
          break;
        case PLQueryCommandEnum.ProductWatchQuery:
          body = "Q6";
          break;
        case PLQueryCommandEnum.HardwareSoftwarePartNumber:
          body = "Q7";
          break;
        case PLQueryCommandEnum.RequestBDTAvailableFeatures:
          body = "Q8";
          break;
        case PLQueryCommandEnum.FuelLevelQuery:
          body = "Q9";
          break;
        case PLQueryCommandEnum.DeviceIDQuery:
          body = "QA";
          break;
        case PLQueryCommandEnum.J1939EventDiagnosticQuery:
          body = "QB";
          break;
        case PLQueryCommandEnum.Deregistration:
          body = "2";
          break;
        case PLQueryCommandEnum.ClearEvents:
          body = "C";
          break;
        case PLQueryCommandEnum.ProductWatchActivateDeactivate:
          body = "P";
          break;
        case PLQueryCommandEnum.RegistrationRequest:
          body = "R";
          break;
        case PLQueryCommandEnum.R2RegistrationRequest:
          body = "R2";
          break;
        case PLQueryCommandEnum.ForcedDeregistration:
          body = "X";
          break;
        case PLQueryCommandEnum.BillingEnable:
          body = "E";
          break;
        case PLQueryCommandEnum.BillingDisable:
          body = "D";
          break;
        case PLQueryCommandEnum.InitialUpgradeRequest:
          body = "U1";
          break;
        case PLQueryCommandEnum.UpgradeRequest:
          body = "U2";
          break;
        case PLQueryCommandEnum.InitialDowngradeRequest:
          body = "K1";
          break;
        case PLQueryCommandEnum.DowngradeRequest:
          body = "K2";
          break;
      }
      
      return AddTOPLOut(opCtx1, moduleCode, subject, body);
    }

    public bool SendGeoFenceConfig(string moduleCode, bool inclusiveProductWatch,
      decimal inclusiveLatitude, decimal inclusiveLongitude, decimal inclusiveRadiusKilometers, 
      List<decimal> exclusiveLatitude, List<decimal> exclusiveLongitude, List<decimal> exclusiveRadius)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      bool exclusiveProductWatch = false;
      if (((exclusiveLatitude.Count() > 5)
             || (exclusiveLongitude.Count() > 5)
             || (exclusiveRadius.Count() > 5)))
      {
        throw new InvalidOperationException("There cannot be more than 5 Exclusive GeoFences");
      }
      
      if (((exclusiveLatitude.Count() > 0)
             || (exclusiveLongitude.Count() > 0)
             || (exclusiveRadius.Count() > 0)))
      {
        exclusiveProductWatch = true;
      }

      //Get the next sequence ID for this device(used for inclusiveMessageID and exclusiveMessageID)
      byte? msgID;
      bool success;
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
        {
            var plOutMessages = (from po in opCtx1.PLOutReadOnly
                where po.ModuleCode == moduleCode
                      && po.SequenceID != null
                orderby po.ID descending
                //use ID instead of InsertUTC because it is indexed and faster
                select po).ToList<PLOut>();

            var desiredPlOutMessage = plOutMessages.Find(IsFenceConfig);
            msgID = desiredPlOutMessage != null ? desiredPlOutMessage.SequenceID : null;

            byte? geofenceMsgID = msgID.HasValue ? (byte) ((msgID.Value + 1)%255) : (byte) 0;

            string body =
                PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatFenceConfig(inclusiveProductWatch,
                    exclusiveProductWatch, false,
                    geofenceMsgID.Value, inclusiveLatitude, inclusiveLongitude, inclusiveRadiusKilometers, null, null,
                    geofenceMsgID.Value, exclusiveLatitude, exclusiveLongitude, exclusiveRadius,
                    0, false, false, false, false, false, false, false, 0x00, 0x00));
            success = AddTOPLOut(opCtx1, moduleCode, body, geofenceMsgID);
        }
        return success;
    }

    public bool SendProductWatchActivation(INH_OP opCtx1, string moduleCode, bool? inclusiveWatchActive, bool? exclusiveWatchActive, bool? timeBasedWatchActive)
    {
      string body = PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatProductWatchActivation(inclusiveWatchActive, exclusiveWatchActive, timeBasedWatchActive));
      return AddTOPLOut(opCtx1, moduleCode, "[COMMAND:ACK]", body);
    }

    private bool IsFenceConfig(PLOut plOutMessage)
    {
      return PLTransactionTypeEnum.FenceConfig.Equals(PLMessageBase.GetMessageType(plOutMessage.Body, false));
    }

    public bool SendDefaultTimeFenceConfig(string moduleCode)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      //timeBasedProductWatch set to false to stop PL Device from sending time fence alarms.
      //This has been changed as a short term fix!
      string body = PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatFenceConfig(false, false, false, 0, 0, 0, 0, null, null, 0,
                                                null, null, null, 0, false, false, false, false, false, false, false, 0x00, 0x00));
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            return AddTOPLOut(opCtx1, moduleCode, body);
        }
    }

    public bool Send24by7TimeFenceConfig(string moduleCode)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      string body = PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatFenceConfig(false, false, true, 0, 0, 0, 0, null, null, 0,
                                                null, null, null, 0, false, false, false, false, false, false, false, 0x00, 0x00));
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            return AddTOPLOut(opCtx1, moduleCode, body);
        }
    }

    public bool SendReportIntervalsConfig(INH_OP opCtx1, string moduleCode, DeviceTypeEnum deviceType, TimeSpan? eventIntervals, EventFrequency? level1TransmissionFrequency, EventFrequency? level2TransmissionFrequency, EventFrequency? level3TransmissionFrequency,
      TimeSpan? nextMessageInterval, bool? globalGramEnable, DateTime? reportStartTimeUTC, EventFrequency? diagnosticTransmissionFrequency,
      SMUFuelReporting? smuFuelReporting, bool? startStopConfigEnabled, int? positionReportConfig)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      if(positionReportConfig.HasValue && (positionReportConfig.Value > 4 || positionReportConfig.Value < 0))
        throw new InvalidOperationException("Position Report Config must be between 0 and 4");

      List<PLReportingIntervalsConfigPayload> payload = new List<PLReportingIntervalsConfigPayload>();
      bool success = true;

      payload = PLOutboundFormatter.FormatReportIntervalsConfig(eventIntervals, level1TransmissionFrequency, level2TransmissionFrequency, level3TransmissionFrequency,
          nextMessageInterval, globalGramEnable, reportStartTimeUTC, diagnosticTransmissionFrequency,
          smuFuelReporting, startStopConfigEnabled, positionReportConfig, null, moduleCode, deviceType);

      foreach(var load in payload)
      {
        string body = PLMessageBase.BytesToBinaryString(load.payload);

        if (string.IsNullOrEmpty(body) || body.Length == 1)
          throw new InvalidOperationException("Body is Empty");
        success &= AddTOPLOut(opCtx1, moduleCode, body);
      }

      return success;
    }

    public bool SendRuntimeAdjustmentConfig(string moduleCode, DeviceTypeEnum deviceType, TimeSpan newRuntimeValue)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      string body = PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatRuntimeAdjustment(newRuntimeValue, moduleCode, deviceType));

      if (string.IsNullOrEmpty(body) || body == "01")
        throw new InvalidOperationException("Body is Empty");
        // Not Used anywhere hence opening a new Context
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            return AddTOPLOut(opCtx1, moduleCode, body);
        }
    }

    public bool SendDigitalInputConfig(INH_OP opCtx1, string moduleCode, InputConfig? input1Config, TimeSpan? input1DelayTime,
      DigitalInputMonitoringConditions? digitalInput1MonitoringCondition, string description1, InputConfig? input2Config, TimeSpan? input2DelayTime,
      DigitalInputMonitoringConditions? digitalInput2MonitoringCondition, string description2, InputConfig? input3Config, TimeSpan? input3DelayTime,
      DigitalInputMonitoringConditions? digitalInput3MonitoringCondition, string description3, InputConfig? input4Config, TimeSpan? input4DelayTime,
      DigitalInputMonitoringConditions? digitalInput4MonitoringCondition, string description4)
    {
      if (string.IsNullOrEmpty(moduleCode))
        throw new ArgumentException("Invalid parameters");

      string body = PLMessageBase.BytesToBinaryString(PLOutboundFormatter.FormatDigitalInputParameters(moduleCode, input1Config, input1DelayTime,
      digitalInput1MonitoringCondition, description1, input2Config, input2DelayTime,
      digitalInput2MonitoringCondition, description2, input3Config, input3DelayTime,
      digitalInput3MonitoringCondition, description3, input4Config, input4DelayTime,
      digitalInput4MonitoringCondition, description4));
      if (string.IsNullOrEmpty(body) || body == "01")
        throw new InvalidOperationException("Body is Empty");
      return AddTOPLOut(opCtx1, moduleCode, body);
    }

    private bool AddTOPLOut(INH_OP opCtx1, string moduleCode, string body, byte? sequenceID)
    {
      return AddTOPLOut(opCtx1, moduleCode, string.Empty, body, sequenceID);
    }

    private bool AddTOPLOut(INH_OP opCtx1, string moduleCode, string body)
    {
      return AddTOPLOut(opCtx1, moduleCode, string.Empty, body, null);
    }

    private bool AddTOPLOut(INH_OP opCtx1, string moduleCode, string subject, string body)
    {
      return AddTOPLOut(opCtx1, moduleCode, subject, body, null);
    }

    private bool AddTOPLOut(INH_OP opCtx1, string moduleCode, string subject, string body, byte? sequenceID)
    {
      bool success = true;
      PLDevice device;
          device = (from d in opCtx1.PLDevice
                      where d.ModuleCode == moduleCode
                      select d).FirstOrDefault();

        if (null != device)
        {
          if ((device.GlobalgramEnabled.HasValue && device.GlobalgramEnabled.Value) && device.SatelliteNumber.HasValue)
          {
            subject = string.Format("[GLOBALGRAM:SAT={0}]", device.SatelliteNumber);
          }

          //Set OTADeregistrationSentUTC if a 'Deregistration' command is being sent out 
          if (body == "2")
          {
            device.OTADeregistrationSentUTC = DateTime.UtcNow;
          }

          if (!string.IsNullOrEmpty(device.ModuleCode))
          {
            PLOut plOut = new PLOut();
            plOut.ModuleCode = moduleCode;
            plOut.Subject = string.IsNullOrEmpty(subject) ? null : subject;
            plOut.Body = string.IsNullOrEmpty(body) ? string.Empty : body;
            plOut.InsertUTC = DateTime.UtcNow;
            plOut.Status = (int)MessageStatusEnum.Pending;
            plOut.SequenceID = sequenceID;

            int result;

            opCtx1.PLOut.AddObject(plOut);
            result = opCtx1.SaveChanges();

            if (result <= 0)
              throw new InvalidOperationException(string.Format("Failed to save PL command {0} for module {1}", body, moduleCode));
          }
          else
          {
            throw new InvalidOperationException(string.Format("Failed to save PL command {0} for Unknown module {1}", body, moduleCode));
          }
        }
      return success;
    }
  }

  public class PLReportingIntervalsConfigPayload
  {
    public byte[] payload;
    public PLReportingIntervalsConfigPayload(byte[] pload)
    {
      payload = pload;
    }
  }
}
