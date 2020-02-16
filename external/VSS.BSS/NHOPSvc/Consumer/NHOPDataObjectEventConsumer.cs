using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using VSS.Nighthawk.NHOPSvc.Interfaces.Events;
using log4net;
using MassTransit;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHOPSvc.ConfigStatus;
using VSS.Nighthawk.NHOPSvc.Interfaces.Models;
using VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Nighthawk.NHOPSvc.Consumer
{
  /// <summary>
  /// Consumer for NHOPDataObjects
  /// </summary>
  public class NHOPDataObjectEventConsumer : Consumes<INewNhOPDataObjectEvent>.Context
  {
    private readonly IConfigStatus configStatusSvc;
    protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public NHOPDataObjectEventConsumer(IConfigStatus processor)
    {
      configStatusSvc = processor;
    }

    public virtual void Consume(IConsumeContext<INewNhOPDataObjectEvent> context)
    {
      try
      {
        if (context.Message != null && context.Message.Message != null)
        {
          var data = context.Message.Message as INHOPDataObject;
          ProcessMessage(data);
        }
        else
        {
          Log.IfErrorFormat("Received message that did not have an INHOPDataObject in it");
        }
      }
      catch (Exception e)
      {
        Log.IfError("Unexpected Error Processing message from RabbitMQ", e);
      }
    }

    private void ProcessMessage(INHOPDataObject data)
    {
      if (data.GetType() == typeof(DevicePersonality))
      {
        configStatusSvc.UpdatePersonality(data);
      }
  
      else if (data.GetType() == typeof(ECMInfoList))
      {
        ECMInfoList mtsEcmList = data as ECMInfoList;

        foreach (var a in mtsEcmList.j1939EcmList)
        {
          if (a.J1939Name != null)
          {
            a.DataLink = (byte)DatalinkEnum.SAEJ1939;
          }
        }

        List<MTSEcmInfo> Cdlmtsecminfo = (from a in mtsEcmList.cdlEcmList
                                          select new MTSEcmInfo
                                          {
                                            PartNumber = a.PartNumber,
                                            softwarePartNumber = a.SoftwarePartNumber,
                                            datalink = a.DataLink,
                                            mid1 = a.MID.ToString(),
                                            serialNumber = a.SerialNumber,
                                            actingMasterECM = a.ActingMasterECM,
                                            syncSMUClockSupported = a.SyncSMUClockSupported
                                          }).ToList();
        List<MTSEcmInfo> J1939Public = (from a in mtsEcmList.j1939EcmList
                                        where a.J1939Name != null
                                        select new MTSEcmInfo
                                        {
                                          PartNumber = a.PartNumber,
                                          softwarePartNumber = a.SoftwarePartNumber,
                                          datalink = a.DataLink,
                                          mid1 = a.J1939Name,
                                          serialNumber = a.SerialNumber,
                                          actingMasterECM = a.ActingMasterECM,
                                          syncSMUClockSupported = a.SyncSMUClockSupported,
                                          SourceAddress = a.SourceAddress,
                                        }).ToList();
        List<MTSEcmInfo> J1939Propriety = (from a in mtsEcmList.j1939EcmList
                                           where a.J1939Name == null
                                           select new MTSEcmInfo
                                           {
                                             PartNumber = a.PartNumber,
                                             softwarePartNumber = a.SoftwarePartNumber,
                                             datalink = a.DataLink,
                                             mid1 = a.MID.ToString(),
                                             serialNumber = a.SerialNumber,
                                             actingMasterECM = a.ActingMasterECM,
                                             syncSMUClockSupported = a.SyncSMUClockSupported,
                                             SourceAddress = a.SourceAddress,
                                           }).ToList();
        //List<MTSEcmInfo> mts = J1939Public.Concat<MTSEcmInfo>(Cdlmtsecminfo).Concat<MTSEcmInfo>(J1939Propriety).ToList<MTSEcmInfo>();
        List<MTSEcmInfo> J1939 = J1939Public.Concat<MTSEcmInfo>(J1939Propriety).ToList<MTSEcmInfo>();
        if (J1939 != null && J1939.Count > 0)
        {
          configStatusSvc.UpdateECMInfoThroughDataIn(data.GPSDeviceID, data.DeviceType, J1939, DatalinkEnum.J1939, mtsEcmList.TimeStampUtc) ;
        }
        if (Cdlmtsecminfo != null && Cdlmtsecminfo.Count > 0)
        {
          configStatusSvc.UpdateECMInfoThroughDataIn(data.GPSDeviceID, data.DeviceType, Cdlmtsecminfo, DatalinkEnum.CDL, mtsEcmList.TimeStampUtc);
        }
      }
      else if (data.GetType() == typeof(TireMonitoringStatus))
      {
        TireMonitoringStatus tms = data as TireMonitoringStatus;

        A5N2ConfigData.TMSConfig tmsConfig = new A5N2ConfigData.TMSConfig();
        tmsConfig.IsEnabled = tms.isEnabled;
        tmsConfig.MessageSourceID = tms.SourceMsgID.HasValue ? tms.SourceMsgID.Value : 0;
        tmsConfig.Status = MessageStatusEnum.Acknowledged;
        configStatusSvc.UpdateDeviceConfiguration(tms.GPSDeviceID, tms.DeviceType, tmsConfig);
      }
      else if (data.GetType() == typeof(RuntimeHourMeterAdjustment))
      {
        RuntimeHourMeterAdjustment hma = data as RuntimeHourMeterAdjustment;
        // Update RuntimeAdjustment: CurrentDeviceSMH = newCalibrationHours - Delta.
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {

          List<RunTimeAdjustment> rtAdj = (from cr in ctx.RunTimeAdjustment
                                           where cr.AssetID == hma.AssetID
                                           select cr).ToList();
          if (rtAdj != null && rtAdj.Count > 0)
          {
            rtAdj.ForEach(f => f.CurrentDeviceSMH = f.NewCalibrationHours - f.DeltaHours);
            ctx.SaveChanges();
          }


        }
        long messageId = (long)hma.SourceMsgID;
        RuntimeAdjConfig runtimeConfig = new RuntimeAdjConfig();
        runtimeConfig.Runtime = TimeSpan.FromHours(hma.HoursValue);
        runtimeConfig.Status = MessageStatusEnum.Sent;
        runtimeConfig.SentUTC = DateTime.UtcNow;
        runtimeConfig.MessageSourceID = messageId;
        MTSUpdateDeviceConfig.UpdateRuntimeCalibration(hma.GPSDeviceID, hma.DeviceType, hma.HoursValue, messageId);
      }
      else if (data.GetType() == typeof(AssetSecurityStartModeStatus))
      {
        A5N2ConfigData.AssetSecurityStartStatus newConfig = new A5N2ConfigData.AssetSecurityStartStatus();

        AssetSecurityStartModeStatus assetSecurityStartModeStatus = data as AssetSecurityStartModeStatus;

        if (assetSecurityStartModeStatus == null) return;
        newConfig.MachineStartStatus = assetSecurityStartModeStatus.StartStatus;
        newConfig.MachineStartStatusTrigger = assetSecurityStartModeStatus.StartStatusTrigger;
        newConfig.MachineStartStatusSentUTC = assetSecurityStartModeStatus.TimestampUtc ?? DateTime.UtcNow; 
        newConfig.SentUTC = assetSecurityStartModeStatus.TimestampUtc ?? DateTime.UtcNow;
        if (assetSecurityStartModeStatus.SourceMsgID != null)
          newConfig.MessageSourceID = (long)assetSecurityStartModeStatus.SourceMsgID;
        newConfig.Status = MessageStatusEnum.Acknowledged;        

        configStatusSvc.UpdateDeviceConfiguration(assetSecurityStartModeStatus.GPSDeviceID, assetSecurityStartModeStatus.DeviceType, newConfig);
      }
      else if (data.GetType() == typeof(AssetSecurityTamperResistanceStatus))
      {
        var assetSecurityTamperResistanceStatus = data as AssetSecurityTamperResistanceStatus;
        if (assetSecurityTamperResistanceStatus == null) return;
        var newConfig = new A5N2ConfigData.AssetSecurityTamperLevel
        {
          TamperLevel = assetSecurityTamperResistanceStatus.TamperLevel,
          MessageSourceID = assetSecurityTamperResistanceStatus.SourceMsgID != null ? (long)assetSecurityTamperResistanceStatus.SourceMsgID : 0,
          Status = MessageStatusEnum.Acknowledged,
          TamperLevelSentUtc = assetSecurityTamperResistanceStatus.TimestampUtc ?? DateTime.UtcNow,
          SentUTC = assetSecurityTamperResistanceStatus.TimestampUtc ?? DateTime.UtcNow
        };

        if (!string.IsNullOrEmpty(assetSecurityTamperResistanceStatus.TamperConfigurationSource))
        {
          newConfig.TamperConfigurationSource =
            assetSecurityTamperResistanceStatus.TamperConfigurationSource
              .ToEnum<TamperResistanceModeConfigurationSource>();

        }
        configStatusSvc.UpdateDeviceConfiguration(assetSecurityTamperResistanceStatus.GPSDeviceID, assetSecurityTamperResistanceStatus.DeviceType, newConfig);
      }
      else if (data.GetType() == typeof(AssetSecurityStatus))
      {
        var assetSecurityStatus = data as AssetSecurityStatus;
        if (assetSecurityStatus == null) return;

        var newConfig = new A5N2ConfigData.AssetSecurityStatus { Status = MessageStatusEnum.Acknowledged };
        var tamperResistance = new A5N2ConfigData.AssetSecurityTamperLevel
        {
          TamperLevel = assetSecurityStatus.TamperLevel,
          MessageSourceID =
            assetSecurityStatus.SourceMsgID != null
              ? (long)assetSecurityStatus.SourceMsgID
              : 0,
          Status = MessageStatusEnum.Acknowledged,
          TamperLevelSentUtc =  assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow
        };
        if (!string.IsNullOrEmpty(assetSecurityStatus.TamperConfigurationSource))
        {
          tamperResistance.TamperConfigurationSource =
            assetSecurityStatus.TamperConfigurationSource
              .ToEnum<TamperResistanceModeConfigurationSource>();
        }
        newConfig.AssetSecurityTamperLevel = tamperResistance;
        var startModeStatus = new A5N2ConfigData.AssetSecurityStartStatus
        {
          MachineStartStatus = assetSecurityStatus.StartStatus,
          MachineStartStatusTrigger = assetSecurityStatus.StartStatusTrigger,
          Status = MessageStatusEnum.Acknowledged,
          MachineStartStatusSentUTC = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow
        };
        if (assetSecurityStatus.SourceMsgID != null)
          startModeStatus.MessageSourceID = (long)assetSecurityStatus.SourceMsgID;
        newConfig.AssetSecurityStartStatus = startModeStatus;
        newConfig.SentUTC = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow;
        configStatusSvc.UpdateDeviceConfiguration(assetSecurityStatus.GPSDeviceID, assetSecurityStatus.DeviceType, newConfig);
      }
      else if (data.GetType() == typeof (AssetSecurityPendingStartModeStatus))
      {
        var assetSecurityStatus = data as AssetSecurityPendingStartModeStatus;
        if (assetSecurityStatus == null) return;

        var newConfig = new A5N2ConfigData.AssetSecurityPendingStartStatus
        {
          Status = MessageStatusEnum.Acknowledged, 
          MachineStartStatus = assetSecurityStatus.Status,
          MachineStartModeConfigSource = assetSecurityStatus.ConfigSource,
          MachineStartStatusSentUTC = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow,
          SentUTC = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow
        };

        configStatusSvc.UpdateDeviceConfiguration(assetSecurityStatus.GPSDeviceID, assetSecurityStatus.DeviceType, newConfig);
      }
      else if (data.GetType() == typeof (AssetSecurityPendingStatus))
      {
        var assetSecurityStatus = data as AssetSecurityPendingStatus;
        if (assetSecurityStatus == null) return;

        var newConfig = new A5N2ConfigData.AssetSecurityPendingStatus {Status = MessageStatusEnum.Acknowledged};
        var tamperResistance = new A5N2ConfigData.AssetSecurityTamperLevel
        {
          TamperLevel = assetSecurityStatus.TamperLevel,
          MessageSourceID =
            assetSecurityStatus.SourceMsgID != null ? (long) assetSecurityStatus.SourceMsgID : 0,
            TamperLevelSentUtc = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow,
          Status = MessageStatusEnum.Acknowledged,
        };
        if (!string.IsNullOrEmpty(assetSecurityStatus.TamperConfigurationSource))
        {
          tamperResistance.TamperConfigurationSource =
            assetSecurityStatus.TamperConfigurationSource
              .ToEnum<TamperResistanceModeConfigurationSource>();
        }
        newConfig.AssetSecurityTamperLevel = tamperResistance;
        var startModeStatus = new A5N2ConfigData.AssetSecurityPendingStartStatus
        {
          MachineStartStatus = assetSecurityStatus.PendingStartMode,
          Status = MessageStatusEnum.Acknowledged,
          MachineStartStatusSentUTC =  assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow
        };
        if (assetSecurityStatus.SourceMsgID != null)
          startModeStatus.MessageSourceID = (long) assetSecurityStatus.SourceMsgID;

        if (!string.IsNullOrEmpty(assetSecurityStatus.StartModeConfigSource))
          startModeStatus.MachineStartModeConfigSource =
            assetSecurityStatus.StartModeConfigSource.ToEnum<MachineStartModeConfigurationSource>();
        newConfig.AssetSecurityPendingStartStatus = startModeStatus;
        newConfig.SentUTC = assetSecurityStatus.TimestampUtc ?? DateTime.UtcNow;
        configStatusSvc.UpdateDeviceConfiguration(assetSecurityStatus.GPSDeviceID, assetSecurityStatus.DeviceType,newConfig);
      }
      else if (data.GetType() == typeof(DailyReportFrequency))
      {
        DailyReportFrequency drFrequency = data as DailyReportFrequency;

        A5N2ConfigData.DailyReportFrequencyConfig reportFrequencyConfig = new A5N2ConfigData.DailyReportFrequencyConfig();
        reportFrequencyConfig.ReportFrequency = drFrequency.ReportFrequency;
        reportFrequencyConfig.MessageSourceID = drFrequency.SourceMsgID.HasValue ? drFrequency.SourceMsgID.Value : 0;
        reportFrequencyConfig.Status = MessageStatusEnum.Acknowledged;
        reportFrequencyConfig.SentUTC = DateTime.UtcNow;
        configStatusSvc.UpdateDeviceConfiguration(drFrequency.GPSDeviceID, drFrequency.DeviceType, reportFrequencyConfig);
      }
    }
  }
}
