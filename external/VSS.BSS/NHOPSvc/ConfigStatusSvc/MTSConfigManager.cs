using log4net;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Nighthawk.NHOPSvc.ConfigStatus
{
  public class MTSConfigManager
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly DateTime DefaultSentUTC = new MTSOut().SentUTC;

    private readonly IConfigStatus configStatus;
    private Timer getDataTimer;
    private readonly List<string> tasks;

    public MTSConfigManager(IConfigStatus processor)
    {
      configStatus = processor;
      tasks = new List<string>
                {
                  ConfigStatusSettings.Default.MTSConfigPending,
                  ConfigStatusSettings.Default.MTSConfigAck,
                  ConfigStatusSettings.Default.MTSConfigOther,
                  ConfigStatusSettings.Default.MTSConfigFirmware
                };
    }

    public void Start()
    {
      getDataTimer = new Timer(ProcessMTSMessages);
      getDataTimer.Change(ConfigStatusSettings.Default.GetDataIntervalTimeout, TimeSpan.FromMilliseconds(-1));
    }
    public void Stop()
    {
      if (getDataTimer != null)
        getDataTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void ProcessMTSMessages(object sender)
    {
      try
      {
        List<BookmarkManager> bookmarks = GetBookmarks();
        if (bookmarks == null)
          return;

        BookmarkManager bookmark = bookmarks.FirstOrDefault(e => e.Task == ConfigStatusSettings.Default.MTSConfigPending);
        Log.IfDebugFormat("Processing task {0} starting at ID: {1}", ConfigStatusSettings.Default.MTSConfigPending, bookmark.BookmarkUTC.HasValue ? bookmark.BookmarkUTC.Value.ToString() : string.Empty);
        DateTime? nextPendingID = UpdatePendingConfigsFromOut(bookmark);
        Log.IfDebugFormat("Finished Processing task {0} new ID: {1}", ConfigStatusSettings.Default.MTSConfigPending, nextPendingID.HasValue ? nextPendingID.Value.ToString() : bookmark.BookmarkUTC.HasValue ? bookmark.BookmarkUTC.Value.ToString() : string.Empty);

        bookmark = bookmarks.FirstOrDefault(e => e.Task == ConfigStatusSettings.Default.MTSConfigAck);
        Log.IfDebugFormat("Processing task {0} starting at ID: {1}", ConfigStatusSettings.Default.MTSConfigAck, bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);
        long? nextAckID = UpdateAcknowlededConfigsFromMTSOut(bookmark);
        Log.IfDebugFormat("Finished Processing task {0} new ID: {0}", ConfigStatusSettings.Default.MTSConfigAck, nextAckID.HasValue ? nextAckID.Value.ToString() : bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);

        bookmark = bookmarks.FirstOrDefault(e => e.Task == ConfigStatusSettings.Default.MTSConfigOther);
        Log.IfDebugFormat("Processing task {0} starting at ID: {1}", ConfigStatusSettings.Default.MTSConfigOther, bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);
        long? nextOtherID = UpdateConfigsFromMTSMessage(bookmark);
        Log.IfDebugFormat("Finished Processing task {0} new ID: {0}", ConfigStatusSettings.Default.MTSConfigOther, nextOtherID.HasValue ? nextOtherID.Value.ToString() : bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);

        bookmark = bookmarks.FirstOrDefault(e => e.Task == ConfigStatusSettings.Default.MTSConfigFirmware);
        Log.IfDebugFormat("Processing task {0} starting at ID: {1}", ConfigStatusSettings.Default.MTSConfigFirmware, bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);
        long? nextFirmwareID = UpdateFirmwareStatus(bookmark);
        Log.IfDebugFormat("Finished Processing task {0} new ID: {0}", ConfigStatusSettings.Default.MTSConfigFirmware, nextFirmwareID.HasValue ? nextFirmwareID.Value.ToString() : bookmark.LastProcessedID.HasValue ? bookmark.LastProcessedID.Value.ToString() : string.Empty);

        EndRun(nextPendingID, nextAckID, nextOtherID, nextFirmwareID);
      }
      catch(OptimisticConcurrencyException)
      {
        Log.IfDebugFormat("OptimisticConcurrencyException occurred");
      }
      catch (Exception e)
      {
        Log.IfError("Unexpected Error processing configs", e);
      }
      finally
      {
        getDataTimer.Change(ConfigStatusSettings.Default.GetDataIntervalTimeout, TimeSpan.FromMilliseconds(-1));
      }
    }

    private void EndRun(DateTime? nextPendingID, long? nextAckID, long? nextOtherID, long? nextFirmwareID)
    {
      using(INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        List<BookmarkManager> task = (from b in opCtx.BookmarkManager
                                      where tasks.Contains(b.Task)
                                      select b).ToList();

        if (task.Count == 0)
        {
          Log.IfError("Could not get tasks to end");
          return;
        }

        foreach (BookmarkManager bookmarkManager in task)
        {
          bookmarkManager.InProgress = false;
          bookmarkManager.UpdateServer = Environment.MachineName;
          bookmarkManager.StartUTC = DateTime.UtcNow;
          bookmarkManager.UpdateUTC = DateTime.UtcNow;
          bookmarkManager.DueUTC = DateTime.UtcNow.Add(ConfigStatusSettings.Default.GetDataIntervalTimeout);
          bookmarkManager.BookmarkUTC = bookmarkManager.Task == ConfigStatusSettings.Default.MTSConfigPending &&
                                        nextPendingID.HasValue
                                          ? nextPendingID
                                          : bookmarkManager.BookmarkUTC;
          if (bookmarkManager.Task == ConfigStatusSettings.Default.MTSConfigAck)
            bookmarkManager.LastProcessedID = nextAckID ?? bookmarkManager.LastProcessedID;
          if (bookmarkManager.Task == ConfigStatusSettings.Default.MTSConfigOther)
            bookmarkManager.LastProcessedID = nextOtherID ?? bookmarkManager.LastProcessedID;
          if (bookmarkManager.Task == ConfigStatusSettings.Default.MTSConfigFirmware)
            bookmarkManager.LastProcessedID = nextFirmwareID ?? bookmarkManager.LastProcessedID;

          Log.IfDebugFormat("Finished processing task: {0} next Due: {1}", bookmarkManager.Task, bookmarkManager.DueUTC);
        }

        opCtx.SaveChanges();
      }
    }

    private DateTime? UpdatePendingConfigsFromOut(BookmarkManager id)
    {
      List<MTSOut> outMessages = GetPendingMessagesFromMTSOut(id);
      if (outMessages.Count > 0)
      {
        UpdateConfigs(outMessages, MessageStatusEnum.Sent);

        DateTime maxInsert = outMessages.Max(e => e.InsertUTC);
        DateTime? maxSent = outMessages.Max(e => e.SentUTC);

        return maxSent > maxInsert ? maxSent : maxInsert;
      }
      return null;
    }

    private void UpdateConfigs(IEnumerable<MTSOut> outMessages, MessageStatusEnum status)
    {
      if (outMessages != null)
      {
        foreach (MTSOut outMessage in outMessages)
        {
          MTSUpdateDeviceConfig.UpdateDeviceStatus(outMessage.ID, (DeviceTypeEnum) outMessage.DeviceType,
                                                   outMessage.SentUTC,
                                                   status, outMessage.SerialNumber, outMessage.PacketID,
                                                   outMessage.Payload, configStatus);
        }
      }
    }

    private List<MTSOut> GetPendingMessagesFromMTSOut(BookmarkManager id)
    {
      if (id != null)
      {
        List<int> messageStatus = new List<int> {(int) MessageStatusEnum.Pending, (int) MessageStatusEnum.Sent};
        List<MTSOut> outMsgs;
        DateTime? lastSentUTC = id.BookmarkUTC ?? DateTime.UtcNow;
        using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
        {
            outMsgs = (from o in opCtx1.MTSOutReadOnly
                     where o.DeviceType != (int)DeviceTypeEnum.MANUALDEVICE &&
                       !o.IsAck && (o.SentUTC > lastSentUTC || (o.SentUTC == DefaultSentUTC && o.InsertUTC > lastSentUTC)) &&
                       (messageStatus.Contains(o.Status))
                     select o).Take(ConfigStatusSettings.Default.MaxNumberToTake).ToList();
        }

        return outMsgs;
      }

      return null;
    }

    private long? UpdateAcknowlededConfigsFromMTSOut(BookmarkManager id)
    {
      long? newID = null;
      List<MTSOut> messages = GetAckedConfigs(id.LastProcessedID ?? 0, ref newID);
      if (messages.Count > 0)
      {
        UpdateConfigs(messages, MessageStatusEnum.Acknowledged);
      }
      return newID;
    }

    private static List<MTSOut> GetAckedConfigs(long mtsMessageID, ref long? newID)
    {
      List<MTSOut> messages = new List<MTSOut>();
      //get messages that have received message responses
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          var allMessages = (from m in opCtx1.MTSMessageReadOnly
                             join o in opCtx1.MTSOutReadOnly on m.SequenceNumber equals o.SequenceID
                    where m.ID > mtsMessageID && m.PacketID == 0x04 && m.SequenceNumber.HasValue && o.SequenceID.HasValue && !o.IsAck
                    select new { mtsOut = o, messageID = m.ID }).ToList();
        var msgs = allMessages.OrderBy(e => e.messageID).Take(ConfigStatusSettings.Default.MaxNumberToTake).ToList();

        if (msgs.Any())
        {
          newID = msgs.Max(i => i.messageID);
          messages = msgs.Select(i => i.mtsOut).ToList();
        }
      }

      return messages;
    }

    private long? UpdateConfigsFromMTSMessage(BookmarkManager id)
    {
      long newID = 0;
      List<MTSMessage> messages = GetMTSMessages(id);

      if(messages.Count > 0)
      {
        newID = ProcessMessages(messages);
      }

      if (newID > 0)
        return newID;

      return null;
    }

    private long ProcessMessages(List<MTSMessage> messages)
    {
      long newID = 0;
      for (int i = 0; i < messages.Count; i++)
      {
        MTSMessage mtsMessage = messages[i];
        //only bother to get the messages that need to be processed by config status svc
        if ((mtsMessage.PacketID == 7 && 
            (((mtsMessage.TypeID & (int)MachineEventTypeEnum.VehicleBusECMInformation) > 0) || 
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.VehicleBusAddressClaim) > 0) || 
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.ECMInfoBlock51) > 0) ||
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.MachineSecurity5302) > 0) || 
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.MaintMode5301) > 0) ||
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.TamperSecurityStatus) > 0) ||
            ((mtsMessage.TypeID & (long)MachineEventTypeEnum.DeviceMachineSecurityReportingStatusMessage) > 0) ||
            ((mtsMessage.TypeID & (int)MachineEventTypeEnum.GatewayAdmin5300) > 0) ||
            ((mtsMessage.TypeID & (Int64)MachineEventTypeEnum.GatewayTMSInfoMessage) > 0))) ||
 
          mtsMessage.PacketID == 0x0F)
        {
          PlatformMessage platformMessage = PlatformMessage.HydratePlatformMessage(mtsMessage.Payload, true, true);
          if (mtsMessage.PacketID == 0x07)
            ProcessMachineEventMessages(mtsMessage.SerialNumber, (DeviceTypeEnum)mtsMessage.DeviceType, platformMessage,
                                        mtsMessage.ID);

          ProcessOpEvents(mtsMessage.SerialNumber, (DeviceTypeEnum)mtsMessage.DeviceType, platformMessage);
        }

        if (i == messages.Count - 1)
          newID = mtsMessage.ID;
      }
      return newID;
    }

    private static List<MTSMessage> GetMTSMessages(BookmarkManager id)
    {
      List<MTSMessage> messages;
      using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          messages = (from m in opCtx1.MTSMessageReadOnly
                    where m.ID > id.LastProcessedID 
                    && m.DeviceType != (int)DeviceTypeEnum.MANUALDEVICE 
                    && (m.PacketID == 7 || m.PacketID == 0x0F)
                    select m).Take(ConfigStatusSettings.Default.MaxNumberToTake).ToList();
      }

      return messages.OrderBy(m=>m.ID).ToList();
    }

    private long? UpdateFirmwareStatus(BookmarkManager id)
    {
      List<MTSBIT> updates = GetFirmwareUpdates(id.LastProcessedID ?? 0);

      if (updates != null && updates.Count > 0)
      {
        foreach (MTSBIT update in updates)
        {
          FirmwareUpdateResponse message =
            PlatformMessage.HydrateBITConfigurationMessageFromBITBinary(update.BlockPayload) as FirmwareUpdateResponse;
          if (message != null)
            configStatus.UpdateFirmwareStatus(update.SerialNumber, (DeviceTypeEnum) update.DeviceType,
                                              (FirmwareUpdateStatusEnum)Enum.Parse(typeof(FirmwareUpdateStatusEnum), message.FirmwareUpdaterStatus.ToString(), true));
        }

        return updates.Max(e => e.ID);
      }

      return null;
    }

    private List<MTSBIT> GetFirmwareUpdates(long id)
    {
      using(INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>())
      {
          return (from b in opCtx1.MTSBITReadOnly where b.DeviceType != (int)DeviceTypeEnum.MANUALDEVICE && b.ID > id && b.BlockID == 0x1B orderby b.ID select b).Take(ConfigStatusSettings.Default.MaxNumberToTake).ToList();
      }
    }

    private List<BookmarkManager> GetBookmarks()
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        List<BookmarkManager> task = (from b in opCtx.BookmarkManager
                                where tasks.Contains(b.Task)
                                select b).ToList();

        if (task.Count == 0)
        {
          Log.IfWarn("Could not get tasks");
          return null;
        }

        BookmarkManager mainTask = task.FirstOrDefault(i => i.Task == ConfigStatusSettings.Default.MTSConfigPending);
        if (mainTask != null && (!mainTask.InProgress || (mainTask.InProgress && mainTask.UpdateServer == Environment.MachineName)
                             || (mainTask.InProgress && mainTask.StartUTC <= DateTime.UtcNow.Subtract(ConfigStatusSettings.Default.ProcessStuckInterval))))
        {
          foreach (BookmarkManager bookmarkManager in task)
          {
            bookmarkManager.InProgress = true;
            bookmarkManager.UpdateServer = Environment.MachineName;
            bookmarkManager.StartUTC = DateTime.UtcNow;
            bookmarkManager.UpdateUTC = DateTime.UtcNow;

            Log.IfDebugFormat("Beginning task: {0}", bookmarkManager.Task);
          }

          opCtx.SaveChanges();
          return task;
        }
      }

      return null;
    }


    private void ProcessMachineEventMessages(string serialNumber, DeviceTypeEnum deviceType, PlatformMessage receivedMessage, long correlationID)
    {
      try
      {
        if (receivedMessage is MachineEventMessage)
        {
          MachineEventMessage machineEvent = receivedMessage as MachineEventMessage;

          foreach (MachineEventBlock block in machineEvent.Blocks)
          {
            if (block.VehicleBusData != null)
            {
              if (block.VehicleBusData is VehicleBusAddressClaimMessage)
              {
                VehicleBusAddressClaimMessage addressClaims = block.VehicleBusData as VehicleBusAddressClaimMessage;
                
                foreach (VehicleBusECMAddressClaim claim in addressClaims.DeviceECMs)
                {
                  configStatus.ProcessAddressClaim(claim.GetECMIDFromJ1939Name(), claim.ArbitraryAddressCapable,
                                                   claim.IndustryGroup,
                                                   claim.VehicleSystemInstance, claim.VehicleSystem,
                                                   claim.Function,
                                                   claim.FunctionInstance, claim.ECUInstance,
                                                   claim.ManufacturerCode,
                                                   claim.IdentityNumber);
                }
              }
            }
            if (block.GatewayData != null)
            {
              GatewayAdministrationMessage message = block.GatewayData as GatewayAdministrationMessage;
              if (message != null && message.Message is TamperSecurityAdministrationInformationMessage)
              {
                MTSUpdateDeviceConfig.UpdateMachineSecuritySystemInformation(serialNumber, deviceType,
                                                                             message.Message as
                                                                             TamperSecurityAdministrationInformationMessage,
                                                                             0);
              }
              else if (message != null && message.Message is MaintenanceAdministrationInformation)
              {
                MTSUpdateDeviceConfig.UpdateMaintenanceMode(serialNumber, deviceType,
                                                            message.Message as MaintenanceAdministrationInformation,
                                                            correlationID, machineEvent.UtcDateTime);
              }

              MachineActivityEventBlock eventBlock = block.GatewayData as MachineActivityEventBlock;
              if (eventBlock != null && eventBlock.Message is TamperSecurityStatusInformationMessage)
              {
                MTSUpdateDeviceConfig.UpdateMachineSecuritySystemInformation(serialNumber, deviceType,
                                                                             eventBlock.Message as
                                                                             TamperSecurityStatusInformationMessage, 0);
              }

              TMSMessageBlock messageBlock = block.GatewayData as TMSMessageBlock;
              if (messageBlock != null && messageBlock.Message is TMSInformationMessage)
              {
                MTSUpdateDeviceConfig.UpdateTMSConfig(serialNumber, 
                                                      deviceType, 
                                                      messageBlock.Message as TMSInformationMessage, 
                                                      correlationID);
              }
            }
            if (block.RadioData != null)
            {
                if (block.RadioData is DeviceMachineSecurityReportingStatusMessage)
                {
                    DeviceMachineSecurityReportingStatusMessage deviceMachineSecurityReportingStatusMessage = block.RadioData as DeviceMachineSecurityReportingStatusMessage;
                    MTSUpdateDeviceConfig.UpdateMachineSecuritySystemInformation(serialNumber, deviceType,
                                                                             deviceMachineSecurityReportingStatusMessage,
                                                                             0);
                    
                }
            }

          }
        }
      }
      catch (Exception e)
      {
        Log.IfError("Error Updating Process MachineEvent Messages data has been dropped", e);
      }
    }

    private void ProcessOpEvents(string serialNumber, DeviceTypeEnum deviceType, PlatformMessage receivedMessage)
    {
      try
      {

        Log.IfInfoFormat("{0} Forwarding {1} to NHOPSvc",
                         serialNumber, receivedMessage == null ? string.Empty : receivedMessage.GetType().Name);
        if (receivedMessage is PersonalityReportMessage)
        {
          ProcessPersonalityReport(serialNumber, deviceType, receivedMessage);
        }
        else if (receivedMessage is BitConfigurationTrackerMessage)
        {
          BitConfigurationTrackerMessage bitMsg = receivedMessage as BitConfigurationTrackerMessage;
          if (bitMsg.BitConfigurationBlocks != null)
          {
            foreach (BitConfigurationMessage bitBlock in bitMsg.BitConfigurationBlocks)
            {
              if (bitBlock.ConfigurationBlockID ==
                  BitConfigurationTrackerMessage.BitConfigurationID.FirmwareUpdateStatus)
              {
                FirmwareUpdateResponse firmwareStatus = bitBlock as FirmwareUpdateResponse;
                if (firmwareStatus != null)
                  UpdateFirmwareStatus(serialNumber, deviceType, firmwareStatus.FirmwareUpdaterStatus.ToString());
              }
            }
          }
        }
        else if (receivedMessage is MachineEventMessage)
        {
          MachineEventMessage machineEvent = receivedMessage as MachineEventMessage;
          if (machineEvent.HydrationErrors == MessageHydrationErrors.NoErrors && machineEvent.Blocks != null &&
              machineEvent.Blocks.Any())
          {
            foreach (MachineEventBlock block in machineEvent.Blocks)
            {
              if (block != null && block.GatewayData != null && block.GatewayData is ECMInformationMessage)
              {
                UpdateECMData(serialNumber, deviceType, block);
              }
              else if (block != null && block.VehicleBusData != null && block.VehicleBusData is VehicleBusECMInformation)
              {
                UpdateVehicleBusECMData(serialNumber, deviceType, block);
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        Log.IfError("Error Updating Process Op Events has been dropped", e);
      }
    }

    private void UpdateVehicleBusECMData(string serialNumber, DeviceTypeEnum deviceType, MachineEventBlock block)
    {
      try
      {
        VehicleBusECMInformation ecmInfo = block.VehicleBusData as VehicleBusECMInformation;
        List<MTSEcmInfo> ecmList = new List<MTSEcmInfo>();
        if (ecmInfo != null)
          foreach (VehicleBusECMInfoData data in ecmInfo.DeviceECMs)
          {
            MTSEcmInfo info = new MTSEcmInfo {datalink = (byte) DeviceIDData.DataLinkType.J1939};
            string mid = ECMAddressClaims.GetEcmIDFromTableWithSourceAddress(serialNumber, data.ECMSourceAddress);
            if (!string.IsNullOrEmpty(mid))
            {
              info.mid1 = mid;
              info.SourceAddress = data.ECMSourceAddress;
              info.serialNumber = data.SerialNumber;
              info.softwarePartNumber = data.SoftwarePartNumber;
              info.syncSMUClockSupported = false;
              info.actingMasterECM = false;
              info.engineSerialNumbers = new[] {string.Empty};
              info.transmissionSerialNumbers = new[] {string.Empty};
              info.eventProtocolVersion = 0;
              info.diagnosticProtocolVersion = 0;
              info.applicationLevel1 = 0;
              info.toolSupportChangeLevel1 = 0;
              info.SoftwareDescription = data.SoftwareDescription;
              info.ReleaseDate = data.SoftwareReleaseDate;
              info.PartNumber = data.PartNumber;

              ecmList.Add(info);
            }
          }

        Log.IfDebugFormat("{0} Sending ECMData To Config Status Svc",
                          serialNumber);

        configStatus.UpdateECMInfo(serialNumber, deviceType, ecmList);
      }
      catch (Exception e)
      {
        Log.IfError("Error Updating VehicleBus ECM Data has been dropped", e);
      }
    }

    private void UpdateECMData(string serialNumber, DeviceTypeEnum deviceType, MachineEventBlock block)
    {
      try
      {
        ECMInformationMessage ecmInfo = block.GatewayData as ECMInformationMessage;
        List<MTSEcmInfo> ecmList = new List<MTSEcmInfo>();
        if (ecmInfo != null)
          foreach (DeviceIDData data in ecmInfo.DeviceData)
          {
            MTSEcmInfo info = new MTSEcmInfo
                                {
                                  transmissionSerialNumbers = ecmInfo.TransmissionSerialNumbers,
                                  engineSerialNumbers = ecmInfo.EngineSerialNumbers,
                                  actingMasterECM = data.ActingMasterECM,
                                  syncSMUClockSupported = data.SyncronizedSMUClockStrategySupported,
                                  diagnosticProtocolVersion = data.DiagnosticProtocolVersion,
                                  eventProtocolVersion = data.EventProtocolVersion,
                                  datalink = (byte) data.ECMType                                  
                                };

            if ((ecmInfo.TransactionVersion == 1 && data.ECMType != DeviceIDData.DataLinkType.Unknown ) || (ecmInfo.TransactionVersion == 2 && data.ECMType != DeviceIDData.DataLinkType.SAEJI939 && data.ECMType != DeviceIDData.DataLinkType.Unknown))
            {
            info.applicationLevel1 = data.Module1ApplicationLevel;
            info.toolSupportChangeLevel1 = data.Module1ServiceToolSupportChangeLevel;
            info.mid1 = data.ModuleID1.ToString();
            }

            if (data.ECMType == DeviceIDData.DataLinkType.CDLAndJ1939 || data.ECMType == DeviceIDData.DataLinkType.All)
            {
              info.applicationLevel2 = data.Module2ApplicationLevel;
              info.toolSupportChangeLevel2 = data.Module2ServiceToolSupportChangeLevel;
              info.mid2 = data.ModuleID2;
            }

            if (data.ECMType != DeviceIDData.DataLinkType.CDL && data.ECMType != DeviceIDData.DataLinkType.Unknown && ecmInfo.TransactionVersion == 2)
            {
              info.SourceAddress = data.ECMSourceAddress;
              info.ArbitraryAddressCapable = data.ArbitraryAddressCapable;
              info.IndustryGroup = data.IndustryGroup;
              info.VehicleSystemInstance = data.VehicleSystemInstance;
              info.VehicleSystem = data.VehicleSystem;
              info.Function = data.Function;
              info.FunctionInstance = data.FunctionInstance;
              info.ECUInstance = data.ECUInstance;
              info.ManufacturerCode = data.ManufacturerCode;
              info.IdentityNumber = data.IdentityNumber;
              info.J1939Name = data.GetECMIDFromJ1939Name();
            }
           
            info.serialNumber = data.ECMSerialNumber;
            info.softwarePartNumber = data.ECMSoftwarePartNumber;
            info.PartNumber = data.ECMHardwarePartNumber;
            ecmList.Add(info);
          }

        Log.IfDebugFormat("{0} Sending ECMData To Config Status Svc",
                          serialNumber);

        configStatus.UpdateECMInfo(serialNumber, deviceType, ecmList);
      }
      catch (Exception e)
      {
        Log.IfError("Error Updating ECM Data has been dropped", e);
      }
    }

    private void UpdateFirmwareStatus(string sn, DeviceTypeEnum deviceType, string firmwareStatus)
    {
      try
      {
        Log.IfDebugFormat("{0} Sending Firmware Status To Config Status Svc",
                          sn);
        FirmwareUpdateStatusEnum firmwareVersion =
          (FirmwareUpdateStatusEnum) Enum.Parse(typeof (FirmwareUpdateStatusEnum), firmwareStatus, true);
        configStatus.UpdateFirmwareStatus(sn, deviceType, firmwareVersion);
      }
      catch (Exception e)
      {
        Log.IfError("Error Updating FirmwareStatus data has been dropped", e);
      }
    }

    private void ProcessPersonalityReport(string serialNumber, DeviceTypeEnum deviceType, PlatformMessage message)
    {
      try
      {
        PersonalityReportMessage msg = message as PersonalityReportMessage;
        XElement e = new XElement("PersonalityReport");
        if (msg != null && msg.PersonalityBlocks != null)
        {
          foreach (PersonalityReportBlock p in msg.PersonalityBlocks)
          {
            XElement firmwareElement = new XElement(p.VersionReportType.ToString(), p.Description);
            e.Add(firmwareElement);
          }
        }
        Log.IfDebugFormat("{0} Sending Firmware Status To Config Status Svc",
                          serialNumber);
        configStatus.UpdatePersonality(serialNumber, deviceType, e.ToString());
      }
      catch(Exception e)
      {
        Log.IfError("Error Processing Personality Report data has been dropped", e);
      }
    }
  }
}
