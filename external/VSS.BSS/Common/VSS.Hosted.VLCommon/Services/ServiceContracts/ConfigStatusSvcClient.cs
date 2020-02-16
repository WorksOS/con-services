using System;
using System.Collections.Generic;
using System.ServiceModel;
using log4net;

using VSS.Hosted.VLCommon;
using System.Configuration;
using System.Linq;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Reflection;
using VSS.Hosted.VLCommon.ServiceContracts;

namespace VSS.Hosted.VLCommon
{
  public partial class ConfigStatusSvcClient : ProcessorClientBase, IConfigStatus
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    private static string msmqAddress = "net.msmq://./private/NHConfigStatus";

    public static void ProcessPLGlobalGramEnabledFieldAndSatelliteNumber(List<GlobalGramSatelliteNumber> globalGramSatelliteNumbers)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange(globalGramSatelliteNumbers);      
    }

    public static void ProcessUpdateFirmwareStatus(string gpsDeviceID, DeviceTypeEnum type, FirmwareUpdateStatusEnum status)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdateFirmwareStatus(gpsDeviceID, type, status);
    }

    public static void ProcessUpdatePersonality(string gpsDeviceID, DeviceTypeEnum type, string xmlString)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdatePersonality(gpsDeviceID, type, xmlString);
    }

    public static void ProcessDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdateDeviceConfiguration(gpsDeviceID, deviceType, config);
    }

    public static void ProcessPLDeviceConfiguration(string gpsDeviceID, MessageStatusEnum status, List<PLConfigData.PLConfigBase> configData)
    {
      // This method overload was created to facilitate processing of a PL device configuration message when no device type information is available,
      // as is the case when a PL121 or PL321 device acknowledge a SMU hour meter update message.  The downstream logic in ConfigStatusScv.UpdatePLDeviceConfiguration
      // for processing the device configuration retrieves the device entity from the NH_OP..Device table using the logic of (PL121 || PL321), thus it is 
      // irrevalent which device type is provided to the ProcessPLDeviceConfiguration method.
      ProcessPLDeviceConfiguration(gpsDeviceID, DeviceTypeEnum.PL121, status, configData);
    }
    
    public static void ProcessPLDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, MessageStatusEnum status, List<PLConfigData.PLConfigBase> configData)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdatePLDeviceConfiguration(gpsDeviceID, deviceType, status, configData);
    }

    public static void ProcessECMInfo(string serialNumber, DeviceTypeEnum type, List<MTSEcmInfo> ecmList)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdateECMInfo(serialNumber, type, ecmList);
    }

    public static void UpdateAddressClaim(string ecmID, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance, 
      byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.ProcessAddressClaim(ecmID, arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
                                 vehicleSystem, function, functionInstance, ecuInstance, manufacturerCode,
                                 identityNumber);
    }

    public static void ProcessUpdatePLConfigurationBulk(List<PLDeviceDetailsConfigInfo> configData)
    {
      ConfigStatusSvcClient client = new ConfigStatusSvcClient();
      client.UpdatePLConfigurationBulk(configData);
    }

    #region IConfigStatusSvc Implementation

    public void UpdateFirmwareStatus(string gpsDeviceID, DeviceTypeEnum type, FirmwareUpdateStatusEnum status)
    {
      object[] methodParams = new object[] { gpsDeviceID, type, status };
      Send("UpdateFirmwareStatus", methodParams);
    }

    public void UpdatePersonality(INHOPDataObject message)
    {
      throw new NotSupportedException();
    }

    public void UpdateECMInfoThroughDataIn(string gpsDeviceID, DeviceTypeEnum deviceType, List<MTSEcmInfo> ecmInfoList, DatalinkEnum datalinkEnum, DateTime? timeStampUtc)
    {
      throw new NotSupportedException();
    }

    public void UpdatePersonality(string gpsDeviceID, DeviceTypeEnum type, string firmwareVersions)
    {
      object[] methodParams = new object[] { gpsDeviceID, type, firmwareVersions };
      Send("UpdatePersonality", methodParams);
    }

    public void UpdateECMInfo(string gpsDeviceID, DeviceTypeEnum type, List<MTSEcmInfo> ecmInfo)
    {
      object[] methodParams = new object[] { gpsDeviceID, type, ecmInfo };
      Send("UpdateECMInfo", methodParams);
    }

    public void UpdateDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config)
    {
      object[] methodParams = new object[] { gpsDeviceID, deviceType, config };
      Send("UpdateDeviceConfiguration", methodParams);
    }

    public void UpdatePLDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, MessageStatusEnum status, List<PLConfigData.PLConfigBase> configData)
    {
      object[] methodParams = new object[] { gpsDeviceID, deviceType, status, configData };
      Send("UpdatePLDeviceConfiguration", methodParams);
    }

    public void ProcessAddressClaim(string ecmID, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
      byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber)
    {
      object[] methodParams = new object[]
                                {
                                  ecmID, arbitraryAddressCapable, industryGroup, vehicleSystemInstance,
                                  vehicleSystem, function, functionInstance, ecuInstance, manufacturerCode,
                                  identityNumber
                                };
      Send("ProcessAddressClaim", methodParams);
    }

    public void ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange(List<GlobalGramSatelliteNumber> globalGramSatelliteNumbers)
    {
      object[] methodParams = new object[] { globalGramSatelliteNumbers };
      Send("ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange", methodParams);
    }

    public void UpdatePLConfigurationBulk(List<PLDeviceDetailsConfigInfo> configData)
    {
      object[] methodParams = new object[] { configData };
      Send("UpdatePLConfigurationBulk", methodParams);
    }

    #endregion

    #region Methods for Sending to Config Status Svc

    private bool Send(string methodName, object[] methodParams)
    {
      List<TransportType> endpoints = CreateTransportList();
      bool success = true;
      for (int i = 0; i < endpoints.Count; i++)
      {
        try
        {
          log.IfDebugFormat("Sending to {0} Endpoint at Address {1}", endpoints[i].bindingType.ToString(), endpoints[i].Address);
          IConfigStatus proxy = GetProxy<IConfigStatus>(endpoints[i]);
          ((IClientChannel)proxy).Open();
          //Used reflection to invoke the method so I could make this a single method to send for each configstatussvc method instead of having to put this in each method
          typeof(IConfigStatus).GetMethods().Where(e => e.Name == methodName).Select(e => e).SingleOrDefault().Invoke(proxy, methodParams);
          ((IClientChannel)proxy).Close();
          log.IfDebugFormat("Send Successfull...");
          break;
        }
        catch (Exception e)
        {
          if (i + 1 == endpoints.Count)
          {
            success = false;
            log.IfErrorFormat(e, "Could not send to {0} Endpoint at Address {1} have Exhausted all endpoints. DATA DROPPED", endpoints[i].bindingType.ToString(), endpoints[i].Address);
          }
          else
          {
            log.IfWarnFormat(e, "Could not send to {0} Endpoint at Address {1}", endpoints[i].bindingType.ToString(), endpoints[i].Address);
          }
        }
      }
      return success;
    }

    private static List<TransportType> CreateTransportList()
    {
      List<TransportType> endpoints = new List<TransportType>();
      TransportType msmq = new TransportType { bindingType = BindingType.MSMQ, Address = msmqAddress, channel = null };
      List<TransportType> tcpTypes = null;
      if (!string.IsNullOrEmpty(remoteNHDataIPs))
      {
        tcpTypes = (from s in remoteNHDataIPs.Split(',')
                    select new TransportType { bindingType = BindingType.TCP, Address = string.Format("net.tcp://{0}/RemoteNhOp/ConfigStatus", s), channel = null }).ToList();
      }
      if (string.IsNullOrEmpty(remoteNHDataIPs) || (!string.IsNullOrEmpty(remoteNHDataIPs) && remoteNHDataIPs.ToLower().Contains(Environment.MachineName.ToLower())))
      {
        if (!string.IsNullOrEmpty(msmqAddress))
          endpoints.Add(msmq);
        if (tcpTypes != null && tcpTypes.Count > 0)
        {
          endpoints.AddRange(tcpTypes);
        }
      }
      else
      {
        if (tcpTypes != null && tcpTypes.Count > 0)
        {
          endpoints.AddRange(tcpTypes);
        }
        if (!string.IsNullOrEmpty(msmqAddress))
          endpoints.Add(msmq);
      }

      return endpoints;
    }

    #endregion
  }
}
