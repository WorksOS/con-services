using AutomationCore.API.Framework.Common;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VSS.Kafka.Factory.Interface;
using VSS.Kafka.Factory.Model;
using VSS.MasterData.Device.AcceptanceTests.Utils.Config;
using VSS.MasterData.Device.AcceptanceTests.Utils.Features.Classes.DeviceService;

namespace VSS.MasterData.Device.AcceptanceTests.Scenarios.DeviceService
{
  public class DeviceServiceSupport : IHandler
  {
    #region Variables

    private static Log4Net Log = new Log4Net(typeof(DeviceServiceSupport));

    public CreateDeviceEvent CreateDeviceEvent = new CreateDeviceEvent();
    public UpdateDeviceEvent UpdateDeviceEvent = new UpdateDeviceEvent();
    public string ResponseString = string.Empty;
    public CreateDeviceModel deviceServiceCreateResponse = null;
    public UpdateDeviceModel deviceServiceUpdateResponse = null;

    #endregion

    #region Constructors

    public DeviceServiceSupport(Log4Net myLog)
    {
      DeviceServiceConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region Post Methods

    public void PostValidCreateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(CreateDeviceEvent);

      try
      {
        string accessToken = DeviceServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(DeviceServiceConfig.DeviceServiceEndpoint, HeaderSettings.PostMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Device Service", e);
        throw new Exception(e + " Got Error While Posting Data To Device Service");
      }
    }

    public void PostValidUpdateRequestToService()
    {
      string requestString = JsonConvert.SerializeObject(UpdateDeviceEvent);

      try
      {
        string accessToken = DeviceServiceConfig.GetValidUserAccessToken();
        LogResult.Report(Log, "log_ForInfo", "Posting the request with Valid Values: " + requestString);
        ResponseString = RestClientUtil.DoHttpRequest(DeviceServiceConfig.DeviceServiceEndpoint, HeaderSettings.PutMethod, accessToken,
           HeaderSettings.JsonMediaType, requestString, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Device Service", e);
        throw new Exception(e + " Got Error While Posting Data To Device Service");
      }
    }


    #endregion


    #region DB Methods

    public bool ValidateDB(string eventType)
    {
      try
      {
        //WaitForDB();
        bool dbResult = false;
        int expectedResult = 1;
        string query = "";
        string firmwarePartNumber;
        string deregisteredUtc;

        if (eventType == "CreateEvent")
        {
          if (CreateDeviceEvent.RadioFirmwarePartNumber != null)
          {
            firmwarePartNumber = CreateDeviceEvent.RadioFirmwarePartNumber;
          }
          else
          {
            firmwarePartNumber = CreateDeviceEvent.FirmwarePartNumber;
          }

          query = string.Format(DeviceServiceMySqlQueries.CreateDeviceDetailsByDeviceUID, CreateDeviceEvent.DeviceUID.ToString().Replace("-", ""), CreateDeviceEvent.DeviceSerialNumber,
              CreateDeviceEvent.DeregisteredUTC.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"), CreateDeviceEvent.ModuleType, CreateDeviceEvent.MainboardSoftwareVersion, firmwarePartNumber,
              CreateDeviceEvent.GatewayFirmwarePartNumber, CreateDeviceEvent.DataLinkType, GetDeviceStateId(CreateDeviceEvent.DeviceState), GetDeviceTypeId(CreateDeviceEvent.DeviceType), CreateDeviceEvent.CellModemIMEI, CreateDeviceEvent.DevicePartNumber, CreateDeviceEvent.CellularFirmwarePartnumber, CreateDeviceEvent.NetworkFirmwarePartnumber, CreateDeviceEvent.SatelliteFirmwarePartnumber);

          if (CreateDeviceEvent.DeregisteredUTC == null)
          {
            query = query.Replace("DeregisteredUTC=''", "DeregisteredUTC is null");
          }

          if (CreateDeviceEvent.ModuleType == null)
          {
            query = query.Replace("ModuleType=''", "ModuleType is null");
          }

          if (CreateDeviceEvent.MainboardSoftwareVersion == null)
          {
            query = query.Replace("MainboardSoftwareVersion=''", "MainboardSoftwareVersion is null");
          }

          if (firmwarePartNumber == null)
          {
            query = query.Replace("FirmwarePartNumber=''", "FirmwarePartNumber is null");
          }

          if (CreateDeviceEvent.GatewayFirmwarePartNumber == null)
          {
            query = query.Replace("GatewayFirmwarePartNumber=''", "GatewayFirmwarePartNumber is null");
          }

          if (CreateDeviceEvent.DataLinkType == null)
          {
            query = query.Replace("DataLinkType=''", "DataLinkType is null");
          }

          if (CreateDeviceEvent.CellModemIMEI == null)
          {
            query = query.Replace("CellModemIMEI=''", "CellModemIMEI is null");
          }

          if (CreateDeviceEvent.DevicePartNumber == null)
          {
            query = query.Replace("DevicePartNumber=''", "DevicePartNumber is null");
          }
          if (CreateDeviceEvent.CellularFirmwarePartnumber == null)
          {
            query = query.Replace("CellularFirmwarePartNUmber=''", "CellularFirmwarePartnumber is null");
          }
          if (CreateDeviceEvent.NetworkFirmwarePartnumber == null)
          {
            query = query.Replace("NetworkFirmwarePartNUmber=''", "NetworkFirmwarePartnumber is null");
          }
          if (CreateDeviceEvent.SatelliteFirmwarePartnumber == null)
          {
            query = query.Replace("SatelliteFirmwarePartNumber=''", "SatelliteFirmwarePartnumber is null");
          }

        }
        if (eventType == "UpdateEvent")
        {
          if (UpdateDeviceEvent.RadioFirmwarePartNumber != null)
          {
            firmwarePartNumber = UpdateDeviceEvent.RadioFirmwarePartNumber;
          }
          else
          {
            firmwarePartNumber = UpdateDeviceEvent.FirmwarePartNumber;
          }

          query = string.Format(DeviceServiceMySqlQueries.UpdateDeviceDetailsByDeviceUID, UpdateDeviceEvent.DeviceUID.ToString().Replace("-", ""), UpdateDeviceEvent.DeviceSerialNumber,
              UpdateDeviceEvent.DeregisteredUTC.ToString("yyyy-MM-ddTHH:mm:ss"), UpdateDeviceEvent.ModuleType, UpdateDeviceEvent.MainboardSoftwareVersion, firmwarePartNumber,
              UpdateDeviceEvent.GatewayFirmwarePartNumber, UpdateDeviceEvent.DataLinkType, GetDeviceStateId(UpdateDeviceEvent.DeviceState), GetDeviceTypeId(UpdateDeviceEvent.DeviceType));

          if (UpdateDeviceEvent.DeregisteredUTC == null)
          {
            query = query.Replace("DeregisteredUTC=''", "DeregisteredUTC is null");
          }

          if (UpdateDeviceEvent.ModuleType == null)
          {
            query = query.Replace("ModuleType=''", "ModuleType is null");
          }

          if (UpdateDeviceEvent.MainboardSoftwareVersion == null)
          {
            query = query.Replace("MainboardSoftwareVersion=''", "MainboardSoftwareVersion is null");
          }

          if (firmwarePartNumber == null)
          {
            query = query.Replace("FirmwarePartNumber=''", "FirmwarePartNumber is null");
          }

          if (UpdateDeviceEvent.GatewayFirmwarePartNumber == null)
          {
            query = query.Replace("GatewayFirmwarePartNumber=''", "GatewayFirmwarePartNumber is null");
          }

          if (UpdateDeviceEvent.DataLinkType == null)
          {
            query = query.Replace("DataLinkType=''", "DataLinkType is null");
          }

        }

        LogResult.Report(Log, "log_ForInfo", "Query: " + query);
        List<string> queryResults = GetSQLResults(query);
        if (queryResults.Count != 0)
        {
          if (queryResults[0] != "")
          {
            LogResult.Report(Log, "log_ForInfo", "Expected Value: " + expectedResult.ToString() + ", Actual Value: " + queryResults[0]);
            dbResult = queryResults[0].Equals(expectedResult.ToString());
          }
          if (dbResult == false)
          {
            LogResult.Report(Log, "log_ForError", "DB Verification Failed");
            return false;
          }
        }
        else
        {
          LogResult.Report(Log, "log_ForError", "No Rows Returned From DB");
        }
        return dbResult;
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
        throw new InvalidDataException("Error Occurred while executing db query");
      }
    }

    public List<string> GetSQLResults(string queryString)
    {
      MySqlDataReader dataReader = null;
      List<string> dbResult = new List<string>();
      using (MySqlConnection mySqlConnection = new MySqlConnection(DeviceServiceConfig.MySqlConnection))
      {
        try
        {
          //Open connection 
          mySqlConnection.Open();
          //Execute the SQL query
          MySqlCommand mySqlCommand = new MySqlCommand(queryString, mySqlConnection);
          //Read the results into a SqlDataReader and store in string variable for later reference
          dataReader = mySqlCommand.ExecuteReader();
          while (dataReader != null && dataReader.Read())
          {
            if (dataReader.HasRows)
            {
              for (int i = 0; i < dataReader.VisibleFieldCount; i++)
              {
                dbResult.Add(dataReader[i].ToString());
              }
            }
            //dataReader.ToString();
          }
        }
        catch (Exception e)
        {
          LogResult.Report(Log, "log_ForError", "Got error while executing db query", e);
          throw new InvalidDataException("Error Occurred while executing db query");
        }
      };
      return dbResult;
    }

    #endregion


    #region Utility

    public string GetDeviceStateId(string deviceState)
    {
      int deviceStateId = 0;
      switch (deviceState)
      {
        case "Installed":
          deviceStateId = (int)DeviceState.Installed;
          break;

        case "Provisioned":
          deviceStateId = (int)DeviceState.Provisioned;
          break;

        case "Subscribed":
          deviceStateId = (int)DeviceState.Subscribed;
          break;

        case "DeregisteredTechnician":
          deviceStateId = (int)DeviceState.DeregisteredTechnician;
          break;

        case "DeregisteredStore":
          deviceStateId = (int)DeviceState.DeregisteredStore;
          break;
      }
      return deviceStateId.ToString();
    }


    public string GetDeviceTypeId(string deviceType)
    {
      int deviceTypeId = 0;
      switch (deviceType)
      {
        case "MANUALDEVICE":
          deviceTypeId = (int)DeviceType.MANUALDEVICE;
          break;

        case "PL121":
          deviceTypeId = (int)DeviceType.PL121;
          break;

        case "PL321":
          deviceTypeId = (int)DeviceType.PL321;
          break;

        case "Series522":
          deviceTypeId = (int)DeviceType.Series522;
          break;

        case "Series523":
          deviceTypeId = (int)DeviceType.Series523;
          break;

        case "Series521":
          deviceTypeId = (int)DeviceType.Series521;
          break;

        case "SNM940":
          deviceTypeId = (int)DeviceType.SNM940;
          break;

        case "CrossCheck":
          deviceTypeId = (int)DeviceType.CrossCheck;
          break;

        case "TrimTrac":
          deviceTypeId = (int)DeviceType.TrimTrac;
          break;

        case "PL420":
          deviceTypeId = (int)DeviceType.PL420;
          break;

        case "PL421":
          deviceTypeId = (int)DeviceType.PL421;
          break;

        case "TM3000":
          deviceTypeId = (int)DeviceType.TM3000;
          break;

        case "TAP66":
          deviceTypeId = (int)DeviceType.TAP66;
          break;

        case "SNM451":
          deviceTypeId = (int)DeviceType.SNM451;
          break;

        case "PL431":
          deviceTypeId = (int)DeviceType.PL431;
          break;

        case "DCM300":
          deviceTypeId = (int)DeviceType.DCM300;
          break;

        case "PL641":
          deviceTypeId = (int)DeviceType.PL641;
          break;

        case "PLE641":
          deviceTypeId = (int)DeviceType.PLE641;
          break;

        case "PLE641PLUSPL631":
          deviceTypeId = (int)DeviceType.PLE641PLUSPL631;
          break;

        case "PLE631":
          deviceTypeId = (int)DeviceType.PLE631;
          break;

        case "PL631":
          deviceTypeId = (int)DeviceType.PL631;
          break;

        case "PL241":
          deviceTypeId = (int)DeviceType.PL241;
          break;

        case "PL231":
          deviceTypeId = (int)DeviceType.PL231;
          break;

        case "BasicVirtualDevice":
          deviceTypeId = (int)DeviceType.BasicVirtualDevice;
          break;

        case "MTHYPHEN10":
          deviceTypeId = (int)DeviceType.MTHYPHEN10;
          break;

        case "XT5060":
          deviceTypeId = (int)DeviceType.XT5060;
          break;

        case "XT4860":
          deviceTypeId = (int)DeviceType.XT4860;
          break;

        case "TTUSeries":
          deviceTypeId = (int)DeviceType.TTUSeries;
          break;

        case "XT2000":
          deviceTypeId = (int)DeviceType.XT2000;
          break;

        case "MTGModularGatewayHYPHENMotorEngine":
          deviceTypeId = (int)DeviceType.MTGModularGatewayHYPHENMotorEngine;
          break;

        case "MTGModularGatewayHYPHENElectricEngine":
          deviceTypeId = (int)DeviceType.MTGModularGatewayHYPHENElectricEngine;
          break;

        case "MCHYPHEN3":
          deviceTypeId = (int)DeviceType.MCHYPHEN3;
          break;

        case "Dummy":
          deviceTypeId = (int)DeviceType.Dummy;
          break;

        case "XT6540":
          deviceTypeId = (int)DeviceType.XT6540;
          break;

        case "XT65401":
          deviceTypeId = (int)DeviceType.XT65401;
          break;

        case "XT65402":
          deviceTypeId = (int)DeviceType.XT65402;
          break;

        case "THREEPDATA":
          deviceTypeId = (int)DeviceType.THREEPDATA;
          break;

        case "PL131":
          deviceTypeId = (int)DeviceType.PL131;
          break;

        case "PL141":
          deviceTypeId = (int)DeviceType.PL141;
          break;

        case "PL440":
          deviceTypeId = (int)DeviceType.PL440;
          break;

        case "PLE601":
          deviceTypeId = (int)DeviceType.PLE601;
          break;

        case "PL161":
          deviceTypeId = (int)DeviceType.PL161;
          break;

        case "PL240":
          deviceTypeId = (int)DeviceType.PL240;
          break;

        case "PL542":
          deviceTypeId = (int)DeviceType.PL542;
          break;

        case "PLE642":
          deviceTypeId = (int)DeviceType.PLE642;
          break;

        case "PLE742":
          deviceTypeId = (int)DeviceType.PLE742;
          break;

        case "SNM941":
          deviceTypeId = (int)DeviceType.SNM941;
          break;

        case "PL240B":
          deviceTypeId = (int)DeviceType.PL240B;
          break;

        case "TAP76":
          deviceTypeId = (int)DeviceType.TAP76;
          break;
      }
      return deviceTypeId.ToString();
    }

    #endregion


    #region Response Verification

    public void VerifyDeviceServiceCreateResponse()
    {
      try
      {
        WaitForKafkaResponseAfterCreate();
        if (CreateDeviceEvent.DeviceUID != null)
          Assert.AreEqual(CreateDeviceEvent.DeviceUID, deviceServiceCreateResponse.CreateDeviceEvent.DeviceUID);


      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Verifying Response", e);
        Assert.Fail("Did not find the event in kafka topic");
      }

    }


    #endregion

    public static void WaitForDB()
    {
      int InitialWaitingTimeForReceivingResponseInSeconds = int.Parse(ConfigurationManager.AppSettings["InitialWaitingTimeForReceivingResponseInSeconds"]);
      LogResult.Report(Log, "log_ForInfo", "Waiting " + InitialWaitingTimeForReceivingResponseInSeconds + " seconds for DB");
      for (int i = 1; i <= InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        Thread.Sleep(1000);
      }
    }

    #region IKVM
    public bool BatchRead
    {
      get
      {
        return false;
      }
    }

    public bool ReadAsync
    {
      get
      {
        return false;
      }
    }

    public void Handle(PayloadMessage message)
    {
      try
      {
        if (message.Value == null || message.Value == "null")
        {
          LogResult.Report(Log, "log_ForInfo", "Kafka Message is Null");
          return;
        }

        string s = message.Value;

        if (CreateDeviceEvent != null && CreateDeviceEvent.ActionUTC != null)
        {
          if (CreateDeviceEvent.ActionUTC.ToString() != null && message.Value.Contains(CreateDeviceEvent.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(CreateDeviceEvent.ReceivedUTC.ToString())
              && CreateDeviceEvent.DeviceUID.ToString() != null && message.Value.Contains(CreateDeviceEvent.DeviceUID.ToString()))
            deviceServiceCreateResponse = JsonConvert.DeserializeObject<CreateDeviceModel>(message.Value);
          LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));

          if (UpdateDeviceEvent != null && UpdateDeviceEvent.ActionUTC != null && UpdateDeviceEvent.DeviceUID != Guid.Empty)
          {
            if (UpdateDeviceEvent.ActionUTC.ToString() != null && message.Value.Contains(UpdateDeviceEvent.ActionUTC.ToString("yyyy-MM-ddThh:mm:ss")) && message.Value.Contains(UpdateDeviceEvent.ReceivedUTC.ToString())
                && UpdateDeviceEvent.DeviceUID.ToString() != null && message.Value.Contains(UpdateDeviceEvent.DeviceUID.ToString()))
              deviceServiceUpdateResponse = JsonConvert.DeserializeObject<UpdateDeviceModel>(message.Value);
            LogResult.Report(Log, "log_ForInfo", string.Format("Response Received With Offset {0}: {1}", message.OffSet, message.Value));
          }
        }

      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Handling Response", e);
        throw new Exception(e + "Got Error While Handling Response");
      }
    }

    public void Handle(List<PayloadMessage> messages)
    {
      int s = messages.Count;
    }
    #endregion

    #region Helpers
    private void WaitForKafkaResponseAfterCreate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (CreateDeviceEvent.DeviceUID != Guid.Empty)
        {
          if (deviceServiceCreateResponse != null)
            break;
        }
        Thread.Sleep(1000);
      }
      if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
        throw new Exception("Got Error While Waiting For Kafka Response");
      }
    }

    private void WaitForKafkaResponseAfterUpdate(bool isPositiveCase = true)
    {
      int i = 0;
      if (!isPositiveCase)
        LogResult.Report(Log, "log_ForInfo", "Expecting No Response From Kafka");
      else
        LogResult.Report(Log, "log_ForInfo", "Waiting " + KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds + " seconds For Kafka Response");
      for (i = 0; i < KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds; i++)
      {
        if (UpdateDeviceEvent.DeviceUID != Guid.Empty)
        {
          if (deviceServiceUpdateResponse != null)
            break;
        }
        Thread.Sleep(1000);
      }
      if (i >= KafkaServicesConfig.InitialWaitingTimeForReceivingResponseInSeconds && isPositiveCase)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Waiting For Kafka Response");
        throw new Exception("Got Error While Waiting For Kafka Response");
      }
    }

    #endregion
  }
}
