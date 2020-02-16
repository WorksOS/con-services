using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Configuration;
using System.Xml.Linq;
using System.Xml;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.ServiceContracts;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Services.MDM;

namespace VSS.Nighthawk.NHOPSvc.ConfigStatus
{
    /// <summary>
    /// Service to intercept communications of configuration data changes.
    /// Changes to 'config' data are first picked up by the data collectors in NH (device Gateways and NH Sync topics).
    /// Rather than have these systems write directly to the NH_OP DB, they communicate this information instead to this
    /// service, and this service is responsible for persisting the information to NH_OP. The reason for this 
    /// architecture is to keep the gateway sub-system as de-coupled as possible from the remainder of NH
    /// to facilitate a possible future seperation of this sub-system as it's own standalone deployed system.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class ConfigStatusSvc : IConfigStatus
    {
        #region Member Fields
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
        private static readonly bool EnableNextGenSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.DeviceAPI.EnableSync"]);
        private static NHHost<ConfigStatusSvc> m_host = null;
        private const string queueName = "NHConfigStatus";
        private const byte cdlandJ1939 = 3;
        private const byte saeji939 = 4;
        private const byte saej1939andcdl = 5;
        private const byte saej1939andj1939 = 6;
        private const byte all = 7;
        #endregion

        #region Service Hosting
        /// <summary>
        /// Starts hosting the service
        /// </summary>
        public void Start()
        {
            MSMQUtils.EnsureMSMQ(queueName, MSMQUtils.QueuePath(queueName), true);
            m_host = new NHHost<ConfigStatusSvc>();
            m_host.StartService();

            ConfigDailyReport.Start();
            ConfigStalePendingCleanupSvc.Start();
        }

        /// <summary>
        /// Stops the service, making it unavailable.
        /// </summary>
        public void Stop()
        {
            if (m_host == null) return;

            m_host.StopService();
            ConfigDailyReport.Stop();
            ConfigStalePendingCleanupSvc.Stop();
        }

        #endregion

        public void SetPLWithinAmericas(List<DataHoursLocation> locations)
        {
            try
            {
                log.IfDebugFormat("Processing {0} Locations..", locations.Count);

                // Single query to get the current status for all module codes that have a position report
                Dictionary<string, bool> deviceInAmericas;
                using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
                {
                    List<string> moduleCodes = locations.Select(sf => sf.GPSDeviceID).ToList();
                    deviceInAmericas = (from p in opCtx1.PLDeviceReadOnly
                                        where moduleCodes.Contains(p.ModuleCode)
                                        select new { p.ModuleCode, p.InAmericas }).ToDictionary(kf => kf.ModuleCode, vf => vf.InAmericas);
                }

                // Find the list of devices that need to have their status updated
                List<PLDevice> devicesToUpdate = new List<PLDevice>();
                foreach (DataHoursLocation loc in locations)
                {
                    if (string.IsNullOrEmpty(loc.GPSDeviceID))
                    {
                        log.IfWarn("GpsDeviceID is empty.  Cannot check if in Americas");
                    }
                    else if (!deviceInAmericas.ContainsKey(loc.GPSDeviceID))
                    {
                        log.IfWarnFormat("GpsDeviceID {0} not found in system.  Cannot check if in Americas", loc.GPSDeviceID);
                    }
                    else
                    {
                        bool isWithinAmericas = API.Site.IsPointInAmericas(loc.Latitude.Value, loc.Longitude.Value);
                        if (deviceInAmericas[loc.GPSDeviceID] != isWithinAmericas)
                        {
                            devicesToUpdate.Add(new PLDevice() { ModuleCode = loc.GPSDeviceID, InAmericas = isWithinAmericas });
                        }
                    }
                }

                // Status updates if required
                if (devicesToUpdate.Count > 0)
                {
                    log.IfInfoFormat("Updating IsAmerica flag for {0} PL devices.", devicesToUpdate.Count);
                    PLDeviceAccess.UpdatePLDeviceState(devicesToUpdate);
                }
            }
            catch (Exception e)
            {
                log.IfError("Processing failure determining PL Orbbcom charging region for PL fleet", e);
            }
        }

        /// <summary>
        /// Performs updates to NH_OP..DeviceFirmwareVersion. This is for tracking the delivery status of firmware updates,
        /// initiated from NH. This facilitates tracking whether or not a firmware update was successful.
        /// </summary>
        /// <param name="gpsDeviceID"></param>
        /// <param name="type"></param>
        /// <param name="status"></param>
        public void UpdateFirmwareStatus(string gpsDeviceID, DeviceTypeEnum type, FirmwareUpdateStatusEnum status)
        {
            try
            {
                if (string.IsNullOrEmpty(gpsDeviceID))
                    throw new InvalidOperationException("GPS Device ID is null or empty.");
                if (!AppFeatureMap.DoesDeviceTypeSupportFeature((int)type, AppFeatureEnum.FirmwareStatusUpdate))
                {
                    log.IfWarnFormat(" SN: {0} DeviceType: {1} firmware cannot be updated, Firmware update not supported by this devicetype", gpsDeviceID, type);
                    return;
                }

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    DeviceFirmwareVersion fv = GetDeviceFirmwareVersion(ctx, gpsDeviceID, type);
                    if (fv != null)
                    {
                        fv.fk_FirmwareUpdateStatusID = (int)status;
                        fv.UpdateStatusUTC = DateTime.UtcNow;

                        if (status == FirmwareUpdateStatusEnum.Successful)
                        {
                            fv.fk_MTS500FirmwareVersionIDInstalled = fv.fk_MTS500FirmwareVersionIDPending;
                        }

                        if (ctx.SaveChanges() < 1)
                        {
                            throw new InvalidOperationException("Could not save changes.");
                        }

                        if (status == FirmwareUpdateStatusEnum.Successful)
                        {
                            API.MTSOutbound.SendPersonalityRequest(ctx, new string[] { gpsDeviceID }, type);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "UpdateStatus, SN: {0} Error Updating Firmware Update status", gpsDeviceID ?? string.Empty);
            }
        }

        /// <summary>
        /// Records updates to the NH_OP..DevicePersonality. This is purely read-only information, being a record
        /// of the current operating firmware versions on the device.
        /// </summary>
        /// <param name="gpsDeviceID"></param>
        /// <param name="type"></param>
        /// <param name="firmwareVersions"></param>
        public void UpdatePersonality(string gpsDeviceID, DeviceTypeEnum type, string firmwareVersions)
        {
            try
            {
                if (string.IsNullOrEmpty(gpsDeviceID))
                    throw new InvalidOperationException("GPS Device ID is null or empty.");

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    FindPersonality(ctx, gpsDeviceID, type, firmwareVersions);
                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "UpdatePersonality, SN: {0} Error Updating Firmware Update status", gpsDeviceID ?? string.Empty);
            }
        }

        /// <summary>
        /// This method is used to process DevicePersonality messages coming through service bus via data in
        /// </summary>
        /// <param name="message"></param> 

        public void UpdatePersonality(INHOPDataObject message)
        {
            try
            {
                DevicePersonality devicePersonalityIncomingMessage = (DevicePersonality)message;
                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    Device device = GetDevice(ctx, message.GPSDeviceID, message.DeviceType);
                    if (device != null)
                    {
                        List<DevicePersonality> devicePersonality = (from dp in ctx.DevicePersonality
                                                                     where dp.fk_DeviceID == ((Device)device).ID
                                                                     select dp).ToList<DevicePersonality>();
                        int personalityType = devicePersonalityIncomingMessage.fk_PersonalityTypeID;
                        DevicePersonality currentPersonality = (from d in devicePersonality
                                                                where d.fk_PersonalityTypeID == personalityType
                                                                select d).FirstOrDefault();
                        if (currentPersonality != null)
                        {
                            currentPersonality.Value = devicePersonalityIncomingMessage.Value;
                            currentPersonality.Description = devicePersonalityIncomingMessage.Description;
                            currentPersonality.UpdateUTC = DateTime.UtcNow;
                        }
                        else
                        {
                            currentPersonality = new DevicePersonality();
                            currentPersonality.Value = devicePersonalityIncomingMessage.Value;
                            currentPersonality.fk_PersonalityTypeID = personalityType;
                            currentPersonality.fk_DeviceID = ((Device)device).ID;
                            currentPersonality.UpdateUTC = DateTime.UtcNow;
                            currentPersonality.Description = devicePersonalityIncomingMessage.Description;
                            ctx.DevicePersonality.AddObject(currentPersonality);
                        }

                        //ctx.SaveChanges();
                        bool updated = (ctx.SaveChanges() > 0);
                        if (updated && EnableNextGenSync)
                        {
                            var DeviceGuid = device.DeviceUID;
                            var updateEvent = new
                            {
                                DeviceUID = (Guid)DeviceGuid,
                                MainboardSoftwareVersion = devicePersonalityIncomingMessage.Description,
                                GatewayFirmwarePartNumber = devicePersonalityIncomingMessage.Description,
                                ActionUTC = DateTime.UtcNow
                            };

                            var result = API.DeviceService.UpdateDevice(updateEvent);
                            if (!result)
                            {
                                log.IfInfoFormat("Error occurred while updating device personality in VSP stack. GpsDeviceID :{0}",
                                  device.GpsDeviceID);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "UpdatePersonality, SN: {0} Error Updating Firmware Updae Status", message.GPSDeviceID ?? string.Empty);
            }
        }


        /// <summary>
        /// Persists updates to NH_OP..Device.DeviceDetailsXML
        /// </summary>
        public void UpdatePLDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, MessageStatusEnum status, List<PLConfigData.PLConfigBase> configData)
        {
            DevicePersonality devicePersonality = null;
            DateTime utcNow = DateTime.UtcNow;
            try
            {
                if (string.IsNullOrEmpty(gpsDeviceID))
                    throw new InvalidOperationException("GPS Device ID is null or empty.");

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    log.IfInfoFormat("Updating config for device {0}, with device type: {1}", gpsDeviceID, deviceType.ToString());
                    Device device = GetDevice(ctx, gpsDeviceID, deviceType);
                    if (device != null)
                    {
                        PLConfigData data = null;
                        if (!string.IsNullOrEmpty(device.DeviceDetailsXML))
                            data = new PLConfigData(device.DeviceDetailsXML);
                        else
                            data = new PLConfigData();

                        log.IfDebugFormat("Data for {0} Before Update = {1}", gpsDeviceID, data.ToXElement().ToString());
                        foreach (PLConfigData.PLConfigBase newConfig in configData)
                        {
                            data.Update(newConfig, status);

                            // Create or update device personality module type if present in general registry message.  (In the case of an SMU adjustment message
                            // the module type value is not present and the device personality update will be by-passed.)
                            if (newConfig.GetType() == typeof(PLConfigData.GeneralRegistry) && status == MessageStatusEnum.Acknowledged && device.fk_DeviceTypeID == (int)DeviceTypeEnum.PL321)
                            {
                                PLConfigData.GeneralRegistry gr = newConfig as PLConfigData.GeneralRegistry;
                                if (gr != null && !String.IsNullOrEmpty(gr.ModuleType))
                                {
                                    devicePersonality = (from d in ctx.DevicePersonality
                                                         where d.fk_DeviceID == device.ID && d.fk_PersonalityTypeID == (int)PersonalityTypeEnum.PL321ModuleType
                                                         select d).SingleOrDefault();


                                    if (devicePersonality == null)
                                    {
                                        devicePersonality = new DevicePersonality
                                        {
                                            fk_DeviceID = device.ID,
                                            fk_PersonalityTypeID = (int)PersonalityTypeEnum.PL321ModuleType,
                                            Value = gr.ModuleType,
                                            UpdateUTC = utcNow
                                        };

                                        ctx.DevicePersonality.AddObject(devicePersonality);
                                    }
                                    else
                                    {
                                        devicePersonality.Value = gr.ModuleType;
                                        devicePersonality.UpdateUTC = utcNow;
                                    }
                                }
                            }
                        }

                        string details = data.ToXElement().ToString();
                        List<Param> updatedValues = new List<Param>();

                        log.IfDebugFormat("Latest details for {0} is {1}", gpsDeviceID, details);

                        if (!string.IsNullOrEmpty(details) && device.DeviceDetailsXML != details)
                        {
                            if (log.IsDebugEnabled)
                                log.IfDebugFormat("Updating DeviceDetailsXML for {0} from {1} to {2}", gpsDeviceID, device.DeviceDetailsXML, details);
                            log.IfInfoFormat("Updating DeviceDetailsXML for {0}", gpsDeviceID);

                            if (EnableNextGenSync)
                            {
                                XElement doc = XElement.Parse(details);
                                XElement dev = null, dev_moduleType = null, dev_softwareInfoElement = null;

                                if (!string.IsNullOrEmpty(device.DeviceDetailsXML))
                                {
                                    dev = XElement.Parse(device.DeviceDetailsXML);
                                    dev_moduleType = dev.Descendants("moduleType").FirstOrDefault();
                                    dev_softwareInfoElement = dev.Descendants("SoftwareInfo").FirstOrDefault();
                                }


                                XElement doc_moduleType = doc.Descendants("moduleType").FirstOrDefault();
                                XElement doc_softwareInfoElement = doc.Descendants("SoftwareInfo").FirstOrDefault();

                                if (doc_moduleType != null)
                                {
                                    if (dev_moduleType != null)
                                    {
                                        if (!string.Equals(doc_moduleType.Value, dev_moduleType.Value))
                                        {
                                            updatedValues.Add(new Param { Name = "ModuleType", Value = doc_moduleType.Value });
                                        }
                                    }
                                }
                                if (doc_softwareInfoElement != null)
                                {
                                    if (dev_softwareInfoElement != null)
                                    {
                                        if (!string.Equals(dev_softwareInfoElement.GetStringAttribute("hc11SoftwarePartNumber"), doc_softwareInfoElement.GetStringAttribute("hc11SoftwarePartNumber")))
                                        {
                                            updatedValues.Add(new Param { Name = "GatewayFirmwarePartNumber", Value = doc_softwareInfoElement.GetStringAttribute("hc11SoftwarePartNumber") });
                                        }

                                        else if (!string.IsNullOrEmpty(dev_softwareInfoElement.GetStringAttribute("modemSoftwarePartNumber")))
                                        {
                                            if (!string.Equals(dev_softwareInfoElement.GetStringAttribute("modemSoftwarePartNumber"), doc_softwareInfoElement.GetStringAttribute("modemSoftwarePartNumber")))
                                            {
                                                updatedValues.Add(new Param { Name = "RadioFirmwarePartNumber", Value = doc_softwareInfoElement.GetStringAttribute("modemSoftwarePartNumber") });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        updatedValues.Add(new Param { Name = "GatewayFirmwarePartNumber", Value = doc_softwareInfoElement.GetStringAttribute("hc11SoftwarePartNumber") });
                                        updatedValues.Add(new Param { Name = "RadioFirmwarePartNumber", Value = doc_softwareInfoElement.GetStringAttribute("modemSoftwarePartNumber") });
                                    }
                                }
                            }
                            device.DeviceDetailsXML = details;
                            device.UpdateUTC = utcNow;
                            device.OldestPendingKeyDate = data.OldestPendingKeyDate;

                            if (ctx.SaveChanges() < 1)
                            {
                                log.IfWarn("No Changes to DeviceDetailsXML were saved");
                            }
                            else
                            {
                                updatedValues.Add(new Param { Name = "DeviceUID", Value = (Guid)device.DeviceUID });
                                updatedValues.Add(new Param { Name = "ActionUTC", Value = DateTime.UtcNow });

                                var updateEvent = updatedValues.ToDictionary(field => field.Name, field => field.Value);

                                var result = API.DeviceService.UpdateDevice(updateEvent);
                                if (!result)
                                {
                                    log.IfInfoFormat("Error occurred while updating deviceDetailsXML state in VSP stack. DeviceId :{0}",
                                      device.ID);
                                }
                            }
                        }
                    }
                    else
                    {
                        log.IfInfoFormat("Could not find device for gpsDeviceID {0}", gpsDeviceID);
                    }
                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "UpdateDeviceConfiguration, SN: {0} Error Updating Device Configuration", gpsDeviceID ?? string.Empty);
            }
        }

        public void UpdateDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config)
        {
            try
            {
                if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.TTConfigData.ToValString())
                {
                    log.IfDebugFormat("Updating TT device configuration");
                    UpdateDeviceConfiguration<TTConfigData>(gpsDeviceID, deviceType, config);
                    return;
                }

                if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.MTSConfigData.ToValString())
                {
                    log.IfDebugFormat("Updating MTS config data");
                    UpdateDeviceConfiguration<MTSConfigData>(gpsDeviceID, deviceType, config);
                    return;
                }

                if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ConfigDataType) == ConfigDataType.A5N2ConfigData.ToValString())
                {
                    log.IfDebugFormat("Updating A5N2 config data");
                    UpdateDeviceConfiguration<A5N2ConfigData>(gpsDeviceID, deviceType, config);
                    return;
                }
                log.IfErrorFormat("Unable to update configuration for device {0}.", gpsDeviceID);
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "Unexpected error updating device config for device SN {0}", gpsDeviceID);
            }
        }
        /// <summary>
        /// This updateEcm Method is invoked when ECM information comes through Datain. 
        /// </summary>
        public void UpdateECMInfoThroughDataIn(string gpsDeviceID, DeviceTypeEnum deviceType, List<MTSEcmInfo> ecmInfoList, DatalinkEnum dataLink, DateTime? timestampUtc)
        {
            try
            {
                log.IfInfoFormat("Updating ECMInfo for GPSDeviceID: {0}, DeviceType: {1}, New ECM List Count: {2}", gpsDeviceID, deviceType.ToString(), ecmInfoList.Count);

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    Device device = GetDevice(ctx, gpsDeviceID, deviceType);

                    if (device != null)
                    {
                        List<ECMInfo> ecmList = (from ecm in ctx.ECMInfo
                                                 .Include("ECMDatalinkInfo")
                                                 .Include("ECMDatalinkInfo.MID")
                                                 where ecm.fk_DeviceID == device.ID &&
                                                 ecm.ECMDatalinkInfo.FirstOrDefault().fk_DatalinkID == (int)dataLink
                                                 select ecm).ToList<ECMInfo>();
                        if (ecmList != null)
                        {
                            log.IfInfoFormat("Device: {0} Currently has {1} ECMs", gpsDeviceID, ecmList.Count());
                            if (ecmList.Count > 0 && !ecmList.Any(t => (t.LastECMInfoUTC ?? DateTime.MinValue) <= timestampUtc))
                            {
                                log.IfInfoFormat("Dropping EcmInfo(s) for DeviceId: {0} which is older than existing ECMInfo", gpsDeviceID);
                                return;
                            }
                        }

                        foreach (MTSEcmInfo ecmInfo in ecmInfoList)
                        {
                            if (ecmInfo != null)
                            {
                                ECMInfo ecm = FillECMInfo(ctx, ecmList, device.ID, ecmInfo);
                                ecm.LastECMInfoUTC = timestampUtc;
                                UpdateDataLinkInfoForDataIn(ctx, ecmInfo, ecm);
                            }
                        }

                        RemoveUnusedECMInfo(ctx, device, ecmList, ecmInfoList);

                        bool updated = (ctx.SaveChanges() > 0);
                        if (updated && EnableNextGenSync)
                        {
                            var DeviceGuid = device.DeviceUID;
                            foreach (MTSEcmInfo ecmInfo in ecmInfoList)
                            {
                                if (ecmInfo != null)
                                {
                                    ECMInfo ecm = FillECMInfo(ctx, ecmList, device.ID, ecmInfo);
                                    if (ecmInfo.datalink != (byte)DatalinkEnum.None)
                                    {
                                        UpdateDataLinkInfo(ctx, ecmInfo, ecm);
                                        foreach (ECMDatalinkInfo a in ecm.ECMDatalinkInfo)
                                        {
                                            var dataLinkName = (from datalink in ctx.DatalinkReadOnly
                                                                join ecmdatalinkinfo in ctx.ECMDatalinkInfoReadOnly on datalink.ID equals ecmdatalinkinfo.fk_DatalinkID
                                                                select datalink.Name).FirstOrDefault();
                                            var updateEvent = new
                                            {
                                                GatewayFirmwarePartNumber = ecm.SoftwarePartNumber,
                                                DataLinkType = dataLinkName,
                                                DeviceUID = (Guid)DeviceGuid
                                            };

                                            var result = API.DeviceService.UpdateDevice(updateEvent);
                                            if (!result)
                                            {
                                                log.IfInfoFormat("Error occurred while updating device softwarePartNumber in VSP stack. gpsDeviceID :{0}",
                                                  gpsDeviceID);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        log.IfWarnFormat("Failed to find device with serial number {0}. Skipping ECMInfo Update...", gpsDeviceID);
                    }
                }
            }
            catch (Exception e)
            {
                log.IfError("Error Saving ECM Info", e);
                throw;
            }
        }

        /// <summary>
        /// Provides a central place to persist updates to NH_OP.ECMInfo.
        /// </summary>
        /// <param name="gpsDeviceID">Serial Number of the Device</param>
        /// <param name="type">DeviceType of the device</param>
        /// <param name="ecmInfoList">List of ECMs sent from the device</param>
        public void UpdateECMInfo(string gpsDeviceID, DeviceTypeEnum type, List<MTSEcmInfo> ecmInfoList)
        {
            try
            {
                log.IfInfoFormat("Updating ECMInfo for GPSDeviceID: {0}, DeviceType: {1}, New ECM List Count: {2}", gpsDeviceID, type.ToString(), ecmInfoList.Count);

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    Device device = GetDevice(ctx, gpsDeviceID, type);
                    var softwarePartNumber = string.Empty;

                    if (device != null)
                    {
                        List<ECMInfo> ecmList = (from ecm in ctx.ECMInfo
                                                 .Include("ECMDatalinkInfo")
                                                 .Include("ECMDatalinkInfo.MID")
                                                 .Include("ECMDatalinkInfo.MID.MIDDesc")
                                                 where ecm.fk_DeviceID == device.ID
                                                 select ecm).ToList<ECMInfo>();
                        if (ecmList != null)
                            log.IfInfoFormat("Device: {0} Currently has {1} ECMs", gpsDeviceID, ecmList.Count());

                        foreach (MTSEcmInfo ecmInfo in ecmInfoList)
                        {
                            if (ecmInfo != null)
                            {
                                ECMInfo ecm = FillECMInfo(ctx, ecmList, device.ID, ecmInfo);

                                if (ecmInfo.datalink != (byte)DatalinkEnum.None)
                                    UpdateDataLinkInfo(ctx, ecmInfo, ecm);
                            }
                        }

                        RemoveUnusedECMInfo(ctx, device, ecmList, ecmInfoList);

                        bool updated = (ctx.SaveChanges() > 0);
                        if (updated && EnableNextGenSync)
                        {
                            var DeviceGuid = device.DeviceUID;
                            foreach (MTSEcmInfo ecmInfo in ecmInfoList)
                            {
                                if (ecmInfo != null)
                                {
                                    ECMInfo ecm = FillECMInfo(ctx, ecmList, device.ID, ecmInfo);
                                    if (ecmInfo.datalink != (byte)DatalinkEnum.None)
                                    {
                                        UpdateDataLinkInfo(ctx, ecmInfo, ecm);
                                        foreach (ECMDatalinkInfo a in ecm.ECMDatalinkInfo)
                                        {
                                            var dataLinkName = (from datalink in ctx.DatalinkReadOnly
                                                                join ecmdatalinkinfo in ctx.ECMDatalinkInfoReadOnly on datalink.ID equals ecmdatalinkinfo.fk_DatalinkID
                                                                select datalink.Name).FirstOrDefault();
                                            var updateEvent = new
                                            {
                                                GatewayFirmwarePartNumber = ecm.SoftwarePartNumber,
                                                DataLinkType = dataLinkName,
                                                DeviceUID = (Guid)DeviceGuid
                                            };

                                            var result = API.DeviceService.UpdateDevice(updateEvent);
                                            if (!result)
                                            {
                                                log.IfInfoFormat("Error occurred while updating device softwarePartNumber in VSP stack. gpsDeviceID :{0}",
                                                  gpsDeviceID);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        log.IfWarnFormat("Failed to find device with serial number {0}. Skipping ECMInfo Update...", gpsDeviceID);
                    }
                }
            }
            catch (Exception e)
            {
                log.IfError("Error Saving ECM Info", e);
                throw;
            }
        }

        public void ProcessAddressClaim(string ecmID, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
          byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
        {
            try
            {
                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    MID mid = (from m in ctx.MID
                               where m.MID1 == ecmID
                               select m).FirstOrDefault();
                    if (mid == null)
                    {
                        log.IfInfoFormat("Adding new MID {0} from received Address claim", ecmID);
                        bool addedNewMID = AddNewJ1939MID(ctx, ecmID, arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
                                                          vehicleSystem, function, functionInstance, ecuInstance, manufacturerCode,
                                                          identityNumber);
                        if (!addedNewMID)
                            AddUnknownJ939Desc(ecmID, function, functionInstance, ctx);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        log.IfDebug("MID has Already been added");
                    }

                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "Could not update MID Table for ECMID: {0}", ecmID);
            }
        }

        public void ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange(List<GlobalGramSatelliteNumber> globalGramSatelliteNumbers)
        {
            try
            {
                log.IfInfoFormat("Processing {0} GlobalGram and Satellite Numbers..", globalGramSatelliteNumbers.Count);

                var distinctItems = (from g in globalGramSatelliteNumbers
                                     where API.Device.IsProductLinkDevice((DeviceTypeEnum)g.DeviceType)
                                     select g).Distinct();

                // Single query to get the current status for all module codes that have a position report
                List<PLDevice> deviceGgSat;
                using (INH_OP opCtx1 = ObjectContextFactory.NewNHContext<INH_OP>(true))
                {
                    List<string> moduleCodes = distinctItems.Select(sf => sf.GPSDeviceID).ToList();
                    deviceGgSat = (from p in opCtx1.PLDeviceReadOnly
                                   where moduleCodes.Contains(p.ModuleCode)
                                   select p).ToList();
                }

                // Evaluate which, if any PL devices have had a change in any of these fields
                List<PLDevice> devicesToUpdate = new List<PLDevice>();
                foreach (GlobalGramSatelliteNumber ggSat in distinctItems)
                {
                    PLDevice deviceMatch = deviceGgSat.Where(x => x.ModuleCode == ggSat.GPSDeviceID).FirstOrDefault();
                    if (deviceMatch == null)
                    {
                        log.IfWarnFormat("Unable to find device information for GPSDeviceId:{0}.  Cannot update globalgram and satellite.", ggSat.GPSDeviceID);
                    }
                    else
                    {
                        log.IfDebugFormat("ProcessGlobalGramEnabledFieldAndSatelliteNumber for device:{0} Global Gram Enabled:{1}, Satellite Number:{2}", ggSat.GPSDeviceID, ggSat.GlobalGramEnabled, ggSat.SatelliteNumber);

                        if (!deviceMatch.GlobalgramEnabled.Equals(ggSat.GlobalGramEnabled)
                            || !deviceMatch.SatelliteNumber.Equals(ggSat.SatelliteNumber))
                        {
                            log.IfDebugFormat("ProcessGlobalGramEnabledFieldAndSatelliteNumber: PLDevice {0} changed globalgram or satellite", ggSat.GPSDeviceID);

                            // Note: Correct value for inAmericas is required as it will be overwritten by the sproc otherwise
                            devicesToUpdate.Add(new PLDevice()
                            {
                                ModuleCode = ggSat.GPSDeviceID,
                                InAmericas = deviceMatch.InAmericas,
                                IsReadOnly = deviceMatch.IsReadOnly,
                                GlobalgramEnabled = ggSat.GlobalGramEnabled,
                                SatelliteNumber = ggSat.SatelliteNumber
                            });
                        }
                    }
                }

                // Update device status if required
                if (devicesToUpdate.Count > 0)
                {
                    log.IfInfoFormat("Updating GlobalGram flag or Satellite Number for {0} PL devices.", devicesToUpdate.Count);
                    PLDeviceAccess.UpdatePLDeviceState(devicesToUpdate);
                }
            }
            catch (Exception e)
            {
                log.IfError("Unexpected error determining or setting global-gram and satellite number properties", e);
            }
        }

        public void UpdatePLConfigurationBulk(List<PLDeviceDetailsConfigInfo> configData)
        {
            try
            {
                // Actually queue the processing steps through the configuration service
                foreach (PLDeviceDetailsConfigInfo info in configData)
                {
                    if (info.IsConfigDataSet)
                    {
                        UpdatePLDeviceConfiguration(info.ModuleCode, info.DeviceType, MessageStatusEnum.Acknowledged, info.ConfigData);
                    }

                    if (info.IsEcmListSet)
                    {
                        UpdateECMInfo(info.ModuleCode, info.DeviceType, info.EcmList);
                    }

                    if (info.IsFirmwareVersionsSet)
                    {
                        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                        {
                            FindPersonality(ctx, info.ModuleCode, info.DeviceType, info.FirmwareVersions);
                        }
                    }
                }

                List<GlobalGramSatelliteNumber> globalGramSatelliteNumbers =
                  (from plDevice in configData
                   where plDevice.IsGlobalGramSet == true
                   select new GlobalGramSatelliteNumber()
                   {
                       DeviceType = (int)plDevice.DeviceType,
                       GPSDeviceID = plDevice.ModuleCode,
                       GlobalGramEnabled = plDevice.GlobalGramEnabled,
                       SatelliteNumber = plDevice.SatelliteNumber
                   }).ToList();
                ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange(globalGramSatelliteNumbers);
            }
            catch (Exception e)
            {
                log.IfError("Bulk update of PL global-gram properties failed due to unhandled exception", e);
            }
        }

        #region Implementation

        private bool AddNewJ1939MID(INH_OP ctx, string ecmID, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
      byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
        {
            List<J1939DefaultMIDDescription> descriptions = GetJ1939DescriptionsFromNameValues(ctx, arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
                                               vehicleSystem, function, functionInstance, ecuInstance, manufacturerCode,
                                               identityNumber);

            if (descriptions != null && descriptions.Count >= 1)
            {
                MID mid = new MID { MID1 = ecmID };
                ctx.MID.AddObject(mid);

                foreach (var ecmDesc in descriptions)
                {
                    MIDDesc desc = new MIDDesc { fk_MIDID = mid.ID, fk_LanguageID = ecmDesc.fk_LanguageID, Description = ecmDesc.Name };
                    ctx.MIDDesc.AddObject(desc);
                }
                log.IfInfoFormat("Adding {0} new descriptions for MID {1}", descriptions.Count, ecmID);
                return true;
            }

            return false;
        }

        private static List<J1939DefaultMIDDescription> GetJ1939DescriptionsFromNameValues(INH_OP ctx, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
          byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
        {
            List<J1939DefaultMIDDescription> descriptions = null;
            List<J1939DefaultMIDDescription> validECMDescriptions = (from j in ctx.J1939DefaultMIDDescriptionReadOnly
                                                                     where j.J1939Function == function
                                                                     select j).ToList();

            if (validECMDescriptions.Count >= 1)
            {
                log.IfDebugFormat("Found {0} descriptions for Function {1}", validECMDescriptions.Count, function);
                descriptions = validECMDescriptions;
            }
            else
            {
                return null;
            }

            List<J1939DefaultMIDDescription> bestName = FindBestDescriptionforJ1939Name(validECMDescriptions, function, arbitraryAddressCapable, industryGroup,
                                                                                        vehicleSystemInstance,
                                                                                        vehicleSystem, functionInstance,
                                                                                        ecuInstance, manufacturerCode,
                                                                                        identityNumber);

            if (bestName != null && bestName.Count >= 1)
            {
                descriptions = bestName;
            }
            else
            {
                descriptions = (from v in validECMDescriptions
                                where v.ArbitraryAddressCapable == null &&
                                  v.ECUInstance == null &&
                                  v.FunctionInstance == null &&
                                  v.IdentityNumber == null &&
                                  v.IndustryGroup == null &&
                                  v.ManufacturerCode == null &&
                                  v.VehicleSystem == null &&
                                  v.VehicleSystemInstance == null
                                select v).ToList();
            }
            return descriptions;
        }

        private static List<J1939DefaultMIDDescription> FindBestDescriptionforJ1939Name(List<J1939DefaultMIDDescription> validECMDescriptions, byte function, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance, byte vehicleSystem,
          byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
        {
            List<J1939DefaultMIDDescription> descriptions = validECMDescriptions;
            List<J1939DefaultMIDDescription> ecms = null;

            ecms = (from s in descriptions where s.IndustryGroup == industryGroup select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat("Found {0} descriptions for Function {1} and industry group {2}", ecms.Count, function, industryGroup);
                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.IndustryGroup == null select s).ToList();
            }

            ecms = (from s in descriptions where s.VehicleSystem == vehicleSystem select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat("Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}",
                                  ecms.Count, function, industryGroup, vehicleSystem);
                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.VehicleSystem == null select s).ToList();
            }

            ecms = (from s in ecms where s.VehicleSystemInstance == vehicleSystemInstance select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance);
                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.VehicleSystemInstance == null select s).ToList();
            }

            ecms = (from s in descriptions where s.FunctionInstance == functionInstance select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}, function Instance {5}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance, functionInstance);

                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.FunctionInstance == null select s).ToList();
            }

            ecms = (from s in descriptions where s.ECUInstance == ecuInstance select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}, function Instance {5}, ecu Instance {6}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance, functionInstance, ecuInstance);
                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.ECUInstance == null select s).ToList();
            }

            ecms = (from s in descriptions where s.ArbitraryAddressCapable == arbitraryAddressCapable select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}, function Instance {5}, ecu Instance {6}, arbitraryAddressCapable {7}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance, functionInstance, ecuInstance, arbitraryAddressCapable);
                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.ArbitraryAddressCapable == null select s).ToList();
            }

            ecms = (from s in descriptions where s.ManufacturerCode == manufacturerCode select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}, function Instance {5}, ecu Instance {6}, arbitraryAddressCapable {7}, manufacturerCode {8}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance, functionInstance,
                  ecuInstance, arbitraryAddressCapable, manufacturerCode);

                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.ManufacturerCode == null select s).ToList();
            }

            ecms = (from s in descriptions where s.IdentityNumber == identityNumber select s).ToList();

            if (ecms.Count >= 1)
            {
                log.IfDebugFormat(
                  "Found {0} descriptions for Function {1}, industry group {2}, vehicle system {3}, vehicle System Instance {4}, function Instance {5}, ecu Instance {6}, arbitraryAddressCapable {7}, manufacturerCode {8}, identityNumber {9}",
                  ecms.Count, function, industryGroup, vehicleSystem, vehicleSystemInstance, functionInstance,
                  ecuInstance, arbitraryAddressCapable, manufacturerCode, identityNumber);

                descriptions = ecms;
            }
            else
            {
                descriptions = (from s in descriptions where s.IdentityNumber == null select s).ToList();
            }

            return descriptions;
        }

        private static void AddUnknownJ939Desc(string ecmID, byte function, byte functionInstance, INH_OP ctx)
        {
            MID mid = new MID { MID1 = ecmID };
            MIDDesc desc = new MIDDesc
            {
                Description =
                  string.Format("Function: {0}", function),
                fk_LanguageID = 1,
                fk_MIDID = mid.ID
            };
            ctx.MID.AddObject(mid);
            ctx.MIDDesc.AddObject(desc);
        }


        private void UpdateDeviceConfiguration<TConfigData>(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config)
      where TConfigData : DeviceConfigData, new()
        {
            try
            {
                log.IfInfoFormat("Updating Device Config for gpsDeviceID: {0}, Config type: {1}, Config: {2}", gpsDeviceID, typeof(TConfigData), config.ToXElement());
                if (string.IsNullOrEmpty(gpsDeviceID))
                    throw new InvalidOperationException("GPS Device ID is null or empty.");

                using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
                {
                    Asset asset = GetAsset(ctx, gpsDeviceID, deviceType);
                    Device device = GetDevice(ctx, gpsDeviceID, deviceType);
                    if (device != null)
                    {
                        TConfigData data = new TConfigData();
                        if (!string.IsNullOrEmpty(device.DeviceDetailsXML))
                            data.Parse(device.DeviceDetailsXML);

                        data.Update(config);
                        string detailXML = data.ToXElement().ToString();
                        if (device.DeviceDetailsXML != detailXML)
                        {
                            device.DeviceDetailsXML = detailXML;
                            device.UpdateUTC = DateTime.UtcNow;


                            bool added = data.AuditConfigChanges(ctx, asset, config);
                            log.IfInfoFormat("Audit logging was {1} for gpsDeviceID: {0}", gpsDeviceID, added ? "added" : "skipped");

                            if (ctx.SaveChanges() < 1)
                            {
                                log.IfWarn("No Changes to DeviceDetailsXML were saved");
                            }
                            else
                            {
                                data.UpdateCurrentStatus(ctx, asset, config);
                            }
                        }
                        else
                        {
                            log.Info("DeviceDetails did Not Change");
                        }
                    }
                    else
                        log.IfInfoFormat("Could not find Device {0}, Device Type: {1}", gpsDeviceID, deviceType.ToString());
                }
            }
            catch (Exception e)
            {
                log.IfErrorFormat(e, "UpdateDeviceConfiguration, SN: {0} Error Updating Device Configuration", gpsDeviceID ?? string.Empty);
            }
        }


        private void RemoveUnusedECMInfo(INH_OP ctx, Device device, List<ECMInfo> ecmList, List<MTSEcmInfo> ecmInfo)
        {
            if (ecmList != null && ecmList.Count() > 0)
            {
                for (int i = ecmList.Count() - 1; i >= 0; i--)
                {
                    MTSEcmInfo foundID = (from mts in ecmInfo
                                          where mts.ecmID == ecmList[i].ID
                                          select mts).FirstOrDefault();
                    if (foundID == null)
                    {
                        log.IfInfoFormat("Removing Unused ECM ID {0}", ecmList[i].ID);
                        List<ECMDatalinkInfo> datalinkInfo = ecmList[i].ECMDatalinkInfo.ToList();

                        ecmList[i].ECMDatalinkInfo.Clear();

                        foreach (ECMDatalinkInfo info in datalinkInfo)
                        {
                            ctx.ECMDatalinkInfo.DeleteObject(info);
                        }

                        device.ECMInfo.Remove(ecmList[i]);
                        ctx.ECMInfo.DeleteObject(ecmList[i]);
                    }
                }
            }
        }

        private static ECMInfo FillECMInfo(INH_OP ctx, List<ECMInfo> ecmList, long deviceID, MTSEcmInfo info)
        {
            ECMInfo ecm = null;

            if (ecmList != null && ecmList.Count > 0)
            {
                ecm = FindEcmInfo(ecmList, info.datalink, info.mid1, info.mid2.ToString(), info.J1939Name);
            }

            if (ecm == null)
            {
                log.IfInfoFormat("Creating New ECM with MID1 = {0}, MID2 = {1}", info.mid1.ToNullString(), !info.mid2.HasValue ? string.Empty : info.mid2.ToString());
                ecm = new ECMInfo
                {
                    IsSyncClockMaster = info.actingMasterECM,
                    HasSMUClock = info.syncSMUClockSupported,
                    Engine1SN = info.engineSerialNumbers == null || info.engineSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.engineSerialNumbers[0]) ? string.Empty : info.engineSerialNumbers[0],
                    Transmission1SN = info.transmissionSerialNumbers == null || info.transmissionSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.transmissionSerialNumbers[0]) ? string.Empty : info.transmissionSerialNumbers[0],
                    EventProtocolVer = info.eventProtocolVersion == 1,
                    DiagnosticProtocolVer = info.diagnosticProtocolVersion == 1,
                    SoftwarePartNumber = info.softwarePartNumber,
                    SerialNumber = info.serialNumber
                };
                ecm.fk_DeviceID = deviceID;
                ecm.SoftwareDescription = info.SoftwareDescription;
                ecm.SoftwareReleaseDate = info.ReleaseDate;
                ecm.PartNumber = info.PartNumber;
                if (info.SourceAddress != null)
                    ecm.SourceAddress = (int)info.SourceAddress;
                if (!string.IsNullOrEmpty(info.J1939Name))
                    ecm.J1939Name = info.J1939Name;



                ctx.ECMInfo.AddObject(ecm);
                info.ecmID = ecm.ID;
            }
            else
            {
                log.IfInfoFormat("Updating ECM ID: {0}", ecm.ID);

                if (ecm.IsSyncClockMaster != info.actingMasterECM)
                    ecm.IsSyncClockMaster = info.actingMasterECM;
                if (ecm.HasSMUClock != info.syncSMUClockSupported)
                    ecm.HasSMUClock = info.syncSMUClockSupported;

                string engine1 = info.engineSerialNumbers == null || info.engineSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.engineSerialNumbers[0]) ? string.Empty : info.engineSerialNumbers[0];
                if (ecm.Engine1SN != engine1)
                    ecm.Engine1SN = info.engineSerialNumbers == null || info.engineSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.engineSerialNumbers[0]) ? string.Empty : info.engineSerialNumbers[0];

                string tran1 = info.transmissionSerialNumbers == null || info.transmissionSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.transmissionSerialNumbers[0]) ? string.Empty : info.transmissionSerialNumbers[0];
                if (ecm.Transmission1SN != tran1)
                    ecm.Transmission1SN = info.transmissionSerialNumbers == null || info.transmissionSerialNumbers.Count() == 0 || string.IsNullOrEmpty(info.transmissionSerialNumbers[0]) ? string.Empty : info.transmissionSerialNumbers[0];
                if (ecm.EventProtocolVer != (info.eventProtocolVersion == 1))
                    ecm.EventProtocolVer = info.eventProtocolVersion == 1;
                if (ecm.DiagnosticProtocolVer != (info.diagnosticProtocolVersion == 1))
                    ecm.DiagnosticProtocolVer = info.diagnosticProtocolVersion == 1;
                if (ecm.SoftwarePartNumber != info.softwarePartNumber)
                    ecm.SoftwarePartNumber = info.softwarePartNumber;
                if (ecm.SerialNumber != info.serialNumber)
                    ecm.SerialNumber = info.serialNumber;
                if (ecm.SoftwareDescription != info.SoftwareDescription)
                    ecm.SoftwareDescription = info.SoftwareDescription;
                if (ecm.SoftwareReleaseDate != info.ReleaseDate)
                    ecm.SoftwareReleaseDate = info.ReleaseDate;
                if (ecm.PartNumber != info.PartNumber)
                    ecm.PartNumber = info.PartNumber;
                if (ecm.SourceAddress != info.SourceAddress)
                    ecm.SourceAddress = info.SourceAddress;
                if (ecm.J1939Name != info.J1939Name)
                    ecm.J1939Name = info.J1939Name;

                info.ecmID = ecm.ID;
            }

            if (info != null && info.engineSerialNumbers != null && info.engineSerialNumbers.Count() >= 2)
            {
                log.IfInfoFormat("Updating ECM with second engine Serial Number: {0}", info.engineSerialNumbers[1]);
                if (ecm.Engine2SN != info.engineSerialNumbers[1])
                    ecm.Engine2SN = info.engineSerialNumbers[1];
            }
            else
            {
                ecm.Engine2SN = null;
            }
            if (info != null && info.transmissionSerialNumbers != null && info.transmissionSerialNumbers.Count() >= 2)
            {
                log.IfInfoFormat("Updating ECM with second Transmission Serial Number: {0}", info.transmissionSerialNumbers[1]);
                if (ecm.Transmission2SN != info.transmissionSerialNumbers[1])
                    ecm.Transmission2SN = info.transmissionSerialNumbers[1];
            }
            else
            {
                ecm.Transmission2SN = null;
            }
            return ecm;
        }

        private static ECMInfo FindEcmInfo(List<ECMInfo> deviceEcmInfo, byte datalink, string mid1, string mid2, string J1939Name)
        {
            ECMInfo ecm = null;
            if (datalink == cdlandJ1939)
            {
                ecm = (from ecmInfo in deviceEcmInfo
                       from e in ecmInfo.ECMDatalinkInfo
                       from l in ecmInfo.ECMDatalinkInfo
                       where e.MID.MID1 == mid1
                       && l.MID.MID1 == mid2
                       select ecmInfo).FirstOrDefault();
                if (ecm != null)
                    log.IfInfoFormat("Found Previously Registered ECM With MID1 = {0} and MID2 = {1}", mid1.ToNullString(), mid2.ToNullString());
            }
            if (ecm == null && datalink == all && !string.IsNullOrEmpty(J1939Name))
            {
                ecm = (from ecmInfo in deviceEcmInfo
                       from e in ecmInfo.ECMDatalinkInfo
                       from l in ecmInfo.ECMDatalinkInfo
                       from s in ecmInfo.ECMDatalinkInfo
                       where e.MID.MID1 == mid1
                       && l.MID.MID1 == mid2
                       && s.MID.MID1 == J1939Name
                       select ecmInfo).FirstOrDefault();
                if (ecm != null)
                    log.IfInfoFormat("Found Previously Registered ECM With MID1 = {0},  MID2 = {1} and J1939Name = {2}", mid1.ToNullString(), mid2.ToNullString(), J1939Name.ToNullString());
            }

            if (!string.IsNullOrEmpty(mid1) && ecm == null)
            {
                ecm = (from ecmInfo in deviceEcmInfo
                       from e in ecmInfo.ECMDatalinkInfo
                       where e.MID.MID1 == mid1
                       select ecmInfo).FirstOrDefault();
                if (ecm != null)
                    log.IfInfoFormat("Found Previously Registered ECM With MID1 = {0}", mid1.ToNullString());
            }

            if ((datalink == cdlandJ1939 || datalink == all) && ecm == null)
            {
                ecm = (from ecmInfo in deviceEcmInfo
                       from e in ecmInfo.ECMDatalinkInfo
                       where e.MID.MID1 == mid2
                       select ecmInfo).FirstOrDefault();
                if (ecm != null)
                    log.IfInfoFormat("Found Previously Registered ECM With MID2 = {0}", mid2.ToNullString());
            }

            if (!string.IsNullOrEmpty(J1939Name) && ecm == null && (datalink == saeji939 || datalink == saej1939andcdl || datalink == saej1939andj1939 || datalink == all))
            {
                ecm = (from ecmInfo in deviceEcmInfo
                       from e in ecmInfo.ECMDatalinkInfo
                       where e.MID.MID1 == J1939Name
                       select ecmInfo).FirstOrDefault();
                if (ecm != null)
                    log.IfInfoFormat("Found Previously Registered ECM With J1939Name = {0}", J1939Name);
            }
            return ecm;
        }

        private static void UpdateDataLinkInfo(INH_OP ctx, MTSEcmInfo info, ECMInfo ecm)
        {
            int datalink1 = (int)DatalinkEnum.None;
            string mid1 = info.mid1;
            int datalink2 = (int)DatalinkEnum.J1939;
            ECMDatalinkInfo datalinkInfo1 = null;
            ECMDatalinkInfo datalinkInfo2 = null;
            ECMDatalinkInfo datalinkInfo3 = null;
            string mid2 = info.mid2.ToString();

            if (info.datalink != saeji939)
            {
                datalink1 = info.datalink == cdlandJ1939 || info.datalink == (byte)DatalinkEnum.CDL || info.datalink == all
                            || info.datalink == saej1939andcdl
                  ? (int)DatalinkEnum.CDL
                  : (int)DatalinkEnum.J1939;


                datalinkInfo1 = (from match in ecm.ECMDatalinkInfo
                                 where match.MID.MID1 == mid1 && match.fk_DatalinkID == datalink1
                                 select match).FirstOrDefault();
            }

            if (!String.IsNullOrWhiteSpace(mid2))
            {
                datalinkInfo2 = (from match in ecm.ECMDatalinkInfo
                                 where match.MID.MID1 == mid2 && match.fk_DatalinkID == datalink2
                                 select match).FirstOrDefault();
            }

            if (!String.IsNullOrWhiteSpace(info.J1939Name) && (info.datalink == saeji939 || info.datalink == saej1939andcdl || info.datalink == saej1939andj1939 || info.datalink == all))
            {
                datalinkInfo3 = (from match in ecm.ECMDatalinkInfo
                                 where match.MID.MID1 == info.J1939Name && match.fk_DatalinkID == (int)DatalinkEnum.SAEJ1939
                                 select match).FirstOrDefault();
            }

            if (info.datalink != saeji939)
            {
                if (datalinkInfo1 == null)
                {
                    log.IfInfoFormat(
                      "Creating new DatalinkInfo Record for MID1: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}",
                      mid1.ToNullString(), ((DatalinkEnum)datalink1).ToString(), info.toolSupportChangeLevel1,
                      info.applicationLevel1);
                    datalinkInfo1 = new ECMDatalinkInfo
                    {
                        SvcToolSupportChangeLevel = (short?)info.toolSupportChangeLevel1,
                        ApplicationLevel = (short?)info.applicationLevel1
                    };

                    datalinkInfo1.fk_DatalinkID = datalink1;

                    MID mid = (from m in ctx.MID
                               where m.MID1 == mid1
                               select m).FirstOrDefault();

                    if (mid == null)
                    {
                        mid = new MID { MID1 = mid1 };
                    }

                    datalinkInfo1.MID = mid;
                    ecm.ECMDatalinkInfo.Add(datalinkInfo1);
                }
                else
                {
                    log.IfInfoFormat(
                      "Updating DatalinkInfo Record for MID1: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}",
                      mid1.ToNullString(), ((DatalinkEnum)datalink1).ToString(), info.toolSupportChangeLevel1,
                      info.applicationLevel1);

                    if (datalinkInfo1.ApplicationLevel != (short)info.applicationLevel1)
                        datalinkInfo1.ApplicationLevel = (short)info.applicationLevel1;
                    if (datalinkInfo1.SvcToolSupportChangeLevel != (short)info.toolSupportChangeLevel1)
                        datalinkInfo1.SvcToolSupportChangeLevel = (short)info.toolSupportChangeLevel1;
                }
            }


            if (datalinkInfo2 == null && !String.IsNullOrWhiteSpace(mid2))
            {
                log.IfInfoFormat("Creating new DatalinkInfo Record for MID2: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink2).ToString(), info.toolSupportChangeLevel2, info.applicationLevel2);

                datalinkInfo2 = new ECMDatalinkInfo { SvcToolSupportChangeLevel = (short?)info.toolSupportChangeLevel2, ApplicationLevel = (short?)info.applicationLevel2 };
                datalinkInfo2.fk_DatalinkID = (int)DatalinkEnum.J1939;

                MID mid = (from m in ctx.MID
                           where m.MID1 == mid2
                           select m).FirstOrDefault();

                if (mid == null)
                {
                    mid = new MID { MID1 = mid2 };
                }
                datalinkInfo2.MID = mid;
                ecm.ECMDatalinkInfo.Add(datalinkInfo2);
            }
            else if (datalinkInfo2 != null)
            {
                log.IfInfoFormat("Updating DatalinkInfo Record for MID2: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink2).ToString(), info.toolSupportChangeLevel2, info.applicationLevel2);

                if (datalinkInfo2.ApplicationLevel != (short?)info.applicationLevel2)
                    datalinkInfo2.ApplicationLevel = (short?)info.applicationLevel2;
                if (datalinkInfo2.SvcToolSupportChangeLevel != (short)info.toolSupportChangeLevel2)
                    datalinkInfo2.SvcToolSupportChangeLevel = (short)info.toolSupportChangeLevel2;
            }

            if (datalinkInfo3 == null && !String.IsNullOrWhiteSpace(info.J1939Name) && (info.datalink == saeji939 || info.datalink == saej1939andcdl || info.datalink == saej1939andj1939 || info.datalink == all))
            {
                log.IfInfoFormat("Creating new DatalinkInfo Record for J1939Name: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", info.J1939Name, ((DatalinkEnum)info.datalink).ToString(), info.toolSupportChangeLevel2, info.applicationLevel2);

                datalinkInfo3 = new ECMDatalinkInfo();
                datalinkInfo3.fk_DatalinkID = (int)DatalinkEnum.SAEJ1939;

                MID mid = (from m in ctx.MID
                           where m.MID1 == info.J1939Name
                           select m).FirstOrDefault();

                if (mid == null)
                {
                    mid = new MID { MID1 = info.J1939Name };


                    List<J1939DefaultMIDDescription> descriptions = GetJ1939DescriptionsFromNameValues(ctx,
                      (bool)info.ArbitraryAddressCapable, (byte)info.IndustryGroup, (byte)info.VehicleSystemInstance,
                      (byte)info.VehicleSystem,
                      (byte)info.Function, (byte)info.FunctionInstance, (byte)info.ECUInstance, (ushort)info.ManufacturerCode,
                      (int)info.IdentityNumber);

                    if (descriptions != null && descriptions.Count >= 1)
                    {
                        MIDDesc desc = null;
                        foreach (var ecmDesc in descriptions)
                        {

                            desc = new MIDDesc
                            {
                                fk_MIDID = mid.ID,
                                fk_LanguageID = ecmDesc.fk_LanguageID,
                                Description = ecmDesc.Name
                            };

                            mid.MIDDesc.Add(desc);
                        }

                        log.IfInfoFormat("Adding {0} new descriptions for MID {1}", descriptions.Count, mid);
                    }
                    else
                    {
                        MIDDesc desc = new MIDDesc
                        {
                            fk_MIDID = mid.ID,
                            fk_LanguageID = 1,
                            Description = String.Format("Function: {0}", info.Function)
                        };

                        log.IfInfoFormat("Adding function {0} as descriptions for MID {1}", info.Function, mid);
                        mid.MIDDesc.Add(desc);
                    }
                }
                datalinkInfo3.MID = mid;
                ecm.ECMDatalinkInfo.Add(datalinkInfo3);
            }

            for (int i = ecm.ECMDatalinkInfo.Count - 1; i >= 0; i--)
            {
                ECMDatalinkInfo ecmDLinkToRemove = ecm.ECMDatalinkInfo.ElementAt(i);
                if ((ecmDLinkToRemove.ID > 0 && (datalinkInfo1 == null || ecmDLinkToRemove.ID != datalinkInfo1.ID))
                  && (datalinkInfo2 == null || ecmDLinkToRemove.ID != datalinkInfo2.ID) && (datalinkInfo3 == null || ecmDLinkToRemove.ID != datalinkInfo3.ID))
                {
                    log.IfInfoFormat("Removing Datalink info record ID {0} that are is longer used on ECM ID {1}", ecmDLinkToRemove.ID, ecm.ID);

                    ecm.ECMDatalinkInfo.Remove(ecmDLinkToRemove);
                    ctx.ECMDatalinkInfo.DeleteObject(ecmDLinkToRemove);
                }
            }
        }

        public static void GetJ1939NameSplitup(string j1939name, out bool arbitraryAddressCapable, out byte industryGroup, out byte vehicleSystemInstance, out byte vehicleSystem, out byte reserved, out byte function,
                              out byte functionInstance, out byte ECUInstance, out ushort manufacturerCode, out int identityNumber)
        {
            identityNumber = 0;
            manufacturerCode = 0;
            ECUInstance = 0;
            functionInstance = 0;
            function = 0;
            reserved = 0;
            vehicleSystem = 0;
            vehicleSystemInstance = 0;
            industryGroup = 0;
            arbitraryAddressCapable = false;

            try
            {
                var J1939Name = Convert.ToUInt64(j1939name);
                identityNumber = (int)(J1939Name & 2097151);

                var extracter = (J1939Name >> 21);
                manufacturerCode = (ushort)(extracter & 2047);

                extracter = (extracter >> 11);
                ECUInstance = (byte)(extracter & 7);

                extracter = (extracter >> 3);
                functionInstance = (byte)(extracter & 31);

                extracter = (extracter >> 5);
                function = (byte)(extracter & 255);

                extracter = (extracter >> 8);
                reserved = (byte)(extracter & 1);

                extracter = (extracter >> 1);
                vehicleSystem = (byte)(extracter & 127);

                extracter = (extracter >> 7);
                vehicleSystemInstance = (byte)(extracter & 15);

                extracter = (extracter >> 4);
                industryGroup = (byte)(extracter & 7);

                extracter = (extracter >> 3);
                var arbitraryAddress = (byte)(extracter & 1);

                arbitraryAddressCapable = arbitraryAddress == 1;
            }

            catch (Exception ex)
            {
                log.IfErrorFormat(ex, "J1939Name splitup Process failed for the Name :- {0}.", j1939name);
            }
        }

        private static void UpdateDataLinkInfoForDataIn(INH_OP ctx, MTSEcmInfo info, ECMInfo ecm)
        {
            string mid1 = info.mid1;
            bool arbitraryAddressCapable;
            byte industryGroup, vehicleSystemInstance, vehicleSystem, reserved;
            byte function, functionInstance, ECUInstance;
            ushort manufactureCode;
            int identityNumber;

            int datalink1 = info.datalink == cdlandJ1939 || info.datalink == (byte)DatalinkEnum.CDL || info.datalink == all
                            || info.datalink == saej1939andcdl ? (int)DatalinkEnum.CDL : (int)DatalinkEnum.J1939;
            int datalink2 = (int)DatalinkEnum.J1939;


            string mid2 = info.mid2.ToString();

            ECMDatalinkInfo datalinkInfo1 = (from match in ecm.ECMDatalinkInfo
                                             where match.MID.MID1 == mid1 && match.fk_DatalinkID == datalink1
                                             select match).FirstOrDefault();

            ECMDatalinkInfo datalinkInfo2 = null;

            if (!String.IsNullOrWhiteSpace(mid2))
            {
                datalinkInfo2 = (from match in ecm.ECMDatalinkInfo
                                 where match.MID.MID1 == mid2 && match.fk_DatalinkID == datalink2
                                 select match).FirstOrDefault();
            }

            if (datalinkInfo1 == null)
            {
                log.IfInfoFormat("Creating new DatalinkInfo Record for MID1: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink1).ToString(), info.toolSupportChangeLevel1, info.applicationLevel1);
                datalinkInfo1 = new ECMDatalinkInfo { SvcToolSupportChangeLevel = (short?)info.toolSupportChangeLevel1, ApplicationLevel = (short?)info.applicationLevel1 };

                datalinkInfo1.fk_DatalinkID = datalink1;

                MID mid = (from m in ctx.MID
                           where m.MID1 == mid1
                           select m).FirstOrDefault();

                if (mid == null)
                {
                    mid = new MID { MID1 = mid1 };
                    if (info.datalink == (byte)DatalinkEnum.SAEJ1939)
                    {
                        GetJ1939NameSplitup(mid1, out arbitraryAddressCapable, out industryGroup, out vehicleSystemInstance, out vehicleSystem, out reserved, out function, out functionInstance, out ECUInstance, out manufactureCode, out identityNumber);

                        List<J1939DefaultMIDDescription> descriptions = GetJ1939DescriptionsFromNameValues(ctx, arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
                                               vehicleSystem, function, functionInstance, ECUInstance, manufactureCode,
                                               identityNumber);

                        if (descriptions != null && descriptions.Count >= 1)
                        {
                            foreach (var ecmDesc in descriptions)
                            {
                                MIDDesc desc = new MIDDesc { fk_MIDID = mid.ID, fk_LanguageID = ecmDesc.fk_LanguageID, Description = ecmDesc.Name };
                                mid.MIDDesc.Add(desc);
                            }
                            log.IfInfoFormat("Adding {0} new descriptions for MID {1}", descriptions.Count, mid);
                        }
                        else
                        {
                            MIDDesc desc = new MIDDesc { fk_MIDID = mid.ID, fk_LanguageID = 1, Description = String.Format("Function: {0}", function) };
                            mid.MIDDesc.Add(desc);
                            log.IfInfoFormat("Adding function {0} as descriptions for MID {1}", function, mid);
                        }


                    }
                }

                datalinkInfo1.MID = mid;
                ecm.ECMDatalinkInfo.Add(datalinkInfo1);
            }
            else
            {
                log.IfInfoFormat("Updating DatalinkInfo Record for MID1: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink1).ToString(), info.toolSupportChangeLevel1, info.applicationLevel1);

                if (datalinkInfo1.ApplicationLevel != (short)info.applicationLevel1)
                    datalinkInfo1.ApplicationLevel = (short)info.applicationLevel1;
                if (datalinkInfo1.SvcToolSupportChangeLevel != (short)info.toolSupportChangeLevel1)
                    datalinkInfo1.SvcToolSupportChangeLevel = (short)info.toolSupportChangeLevel1;
            }


            if (datalinkInfo2 == null && !String.IsNullOrWhiteSpace(mid2))
            {
                log.IfInfoFormat("Creating new DatalinkInfo Record for MID2: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink2).ToString(), info.toolSupportChangeLevel2, info.applicationLevel2);

                datalinkInfo2 = new ECMDatalinkInfo { SvcToolSupportChangeLevel = (short?)info.toolSupportChangeLevel2, ApplicationLevel = (short?)info.applicationLevel2 };
                datalinkInfo2.fk_DatalinkID = (int)DatalinkEnum.J1939;

                MID mid = (from m in ctx.MID
                           where m.MID1 == mid2
                           select m).FirstOrDefault();

                if (mid == null)
                {
                    mid = new MID { MID1 = mid2 };
                }
                datalinkInfo2.MID = mid;
                ecm.ECMDatalinkInfo.Add(datalinkInfo2);
            }
            else if (datalinkInfo2 != null)
            {
                log.IfInfoFormat("Updating DatalinkInfo Record for MID2: {0}, DataLinkType = {1}, toolSupportChangeLevel = {2}, applicationLevel = {3}", mid1.ToNullString(), ((DatalinkEnum)datalink2).ToString(), info.toolSupportChangeLevel2, info.applicationLevel2);

                if (datalinkInfo2.ApplicationLevel != (short?)info.applicationLevel2)
                    datalinkInfo2.ApplicationLevel = (short?)info.applicationLevel2;
                if (datalinkInfo2.SvcToolSupportChangeLevel != (short)info.toolSupportChangeLevel2)
                    datalinkInfo2.SvcToolSupportChangeLevel = (short)info.toolSupportChangeLevel2;
            }

            for (int i = ecm.ECMDatalinkInfo.Count - 1; i >= 0; i--)
            {
                ECMDatalinkInfo ecmDLinkToRemove = ecm.ECMDatalinkInfo.ElementAt(i);
                if ((ecmDLinkToRemove.ID > 0 && (datalinkInfo1 == null || ecmDLinkToRemove.ID != datalinkInfo1.ID))
                  && (datalinkInfo2 == null || ecmDLinkToRemove.ID != datalinkInfo2.ID))
                {
                    log.IfInfoFormat("Removing Datalink info record ID {0} that are is longer used on ECM ID {1}", ecmDLinkToRemove.ID, ecm.ID);

                    ecm.ECMDatalinkInfo.Remove(ecmDLinkToRemove);
                    ctx.ECMDatalinkInfo.DeleteObject(ecmDLinkToRemove);
                }
            }
        }

        private Device GetDevice(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type)
        {
            Device device = null;
            if (API.Device.IsProductLinkDevice(type))
            {
                //type is always == PL121 here..so we don't actually know if the device is PL121 or PL321
                List<Device> deviceList = (from d in ctx.Device
                                           where d.GpsDeviceID == gpsDeviceID
                                           && (d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL121 || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL321)
                                           select d).ToList<Device>();

                if (deviceList.Count > 1) //pick the (hopefully only) 1 subscribed and if both, the most recently updated and subscribed, else, just the most recently updated
                {

                    device =
                      (from d in deviceList
                       where d.fk_DeviceStateID == (int)DeviceStateEnum.Subscribed
                       orderby d.UpdateUTC descending
                       select d).FirstOrDefault() ??
                      (from d in deviceList orderby d.UpdateUTC descending select d).FirstOrDefault();
                }
                else if (deviceList.Count > 0)
                {
                    device = deviceList[0];
                }
            }
            else
            {
                int deviceType = (int)type;
                device = (from d in ctx.Device
                          where d.GpsDeviceID == gpsDeviceID
                             && d.fk_DeviceTypeID == deviceType
                          select d).FirstOrDefault<Device>();
            }

            if (device != null)
                log.IfInfoFormat("Found Device with ID: {0} for GpsDeviceID: {1} and DeviceType {2} (If type is PL121, it could also be a PL321)", device.ID, device.GpsDeviceID, type.ToString());
            else
                log.IfInfoFormat("Could not find device for GpsDeviceID: {0} and DeviceType {1} (If type is PL121 it could also be a PL321)", gpsDeviceID, type.ToString());

            return device;
        }

        private Asset GetAsset(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type)
        {
            Asset asset = (from asst in ctx.Asset
                           where asst.Device.GpsDeviceID == gpsDeviceID &&
                                 asst.Device.fk_DeviceTypeID == (int)type
                           select asst).FirstOrDefault<Asset>();

            if (asset != null)
                log.IfInfoFormat("Found Asset with ID: {0}", asset.AssetID);
            else
                log.IfInfoFormat("Could not find asset for GpsDeviceID: {0} and DeviceType {1}", gpsDeviceID, type.ToString());

            return asset;
        }

        private static DeviceFirmwareVersion GetDeviceFirmwareVersion(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type)
        {
            int deviceType = (int)type;
            long deviceID = (from d in ctx.DeviceReadOnly
                             where d.GpsDeviceID == gpsDeviceID
                       && d.fk_DeviceTypeID == deviceType
                             select d.ID).FirstOrDefault<long>();

            if (deviceID == 0)
            {
                log.IfWarnFormat("GPS Device ID {0} with DeviceType {1} not found. cannot update DeviceFirmwareVersion", gpsDeviceID, type);
                return null;
            }

            DeviceFirmwareVersion fv = (from dfv in ctx.DeviceFirmwareVersion
                                        where dfv.fk_DeviceID == deviceID
                                        select dfv).FirstOrDefault<DeviceFirmwareVersion>();
            if (fv == null)
            {
                log.IfWarnFormat("GPS Device ID {0} with DeviceType {1} not found in DeviceFirmwareVersion table.", gpsDeviceID, type);
            }
            return fv;
        }

        private void FindPersonality(INH_OP ctx, string gpsDeviceID, DeviceTypeEnum type, string firmwareVersionsXML)
        {
            int deviceType = (int)type;

            long deviceID = (from d in ctx.DeviceReadOnly
                             where d.GpsDeviceID == gpsDeviceID
                                && d.fk_DeviceTypeID == deviceType
                             select d.ID).FirstOrDefault<long>();

            string bssID = (from d in ctx.DeviceReadOnly
                            where d.GpsDeviceID == gpsDeviceID
                            select d.OwnerBSSID).FirstOrDefault();

            if (deviceID > 0)
            {
                XElement element = XElement.Parse(firmwareVersionsXML);
                var newPersonality = (from d in element.Elements()
                                      select new { type = d.Name.ToString(), Description = d.IsEmpty ? null : d.Value });

                List<DevicePersonality> devicePersonality = (from dp in ctx.DevicePersonality
                                                             where dp.fk_DeviceID == deviceID
                                                             select dp).ToList<DevicePersonality>();

                foreach (var p in newPersonality)
                {
                    PersonalityTypeEnum pType = (PersonalityTypeEnum)Enum.Parse(typeof(PersonalityTypeEnum), p.type);
                    int personalityType = (int)pType;
                    DevicePersonality currentPersonality = (from d in devicePersonality
                                                            where d.fk_PersonalityTypeID == personalityType
                                                            select d).FirstOrDefault();
                    if (currentPersonality != null)
                    {
                        currentPersonality.Value = p.Description;
                        currentPersonality.UpdateUTC = DateTime.UtcNow;
                    }
                    else
                    {
                        currentPersonality = new DevicePersonality();
                        currentPersonality.Value = p.Description;
                        currentPersonality.fk_PersonalityTypeID = personalityType;
                        currentPersonality.fk_DeviceID = deviceID;
                        currentPersonality.UpdateUTC = DateTime.UtcNow;
                        ctx.DevicePersonality.AddObject(currentPersonality);
                    }
                }
                bool updated = (ctx.SaveChanges() > 0);
                if (updated && EnableNextGenSync)
                {
                    var DeviceGuid = (from device in ctx.Device
                                      where device.GpsDeviceID == gpsDeviceID
                                      select device.DeviceUID).FirstOrDefault();

                    var updateEvent = new
                    {
                        MainboardSoftwareVersion = newPersonality.LastOrDefault().Description,
                        GatewayFirmwarePartNumber = newPersonality.LastOrDefault().Description,
                        DeviceUID = (Guid)DeviceGuid,
                        ActionUTC = DateTime.UtcNow,
                    };

                    var result = API.DeviceService.UpdateDevice(updateEvent);
                    if (!result)
                    {
                        log.IfInfoFormat("Error occurred while updating device personality in VSP stack. GpsDeviceID :{0}",
                        gpsDeviceID);
                    }
                }
            }
        }
        #endregion
    }
}
