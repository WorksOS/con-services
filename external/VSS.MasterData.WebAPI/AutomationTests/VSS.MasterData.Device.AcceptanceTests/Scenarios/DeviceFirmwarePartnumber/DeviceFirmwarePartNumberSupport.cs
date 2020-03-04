using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AutomationCore.API.Framework.Library;
using AutomationCore.Shared.Library;
using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json;
using AutomationCore.API.Framework.Common;
using System.Net;
using VSS.MasterData.Device.AcceptanceTests.Utils.Config;

namespace VSS.MasterData.Device.AcceptanceTests.Scenarios.DeviceFirmwarePartnumber
{
  public class DeviceFirmwarePartNumberSupport
  {
    public static int ExpectedResult = 1;

    public static List<string> GetSQLResults(string queryString)
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

          throw new InvalidDataException("Error Occurred while executing db query");
        }
      };
      return dbResult;
    }


    public static bool ValidateDB(string query, int expectedResult)
    {
      bool dbResult = false;
      try
      {
        //WaitForDB();

        //int expectedResult = 1;  

        LogResult.Report(DeviceFirmwarePartNumberSteps.Log, "log_ForInfo", "Query: " + query);
        // List<string> queryResults = new List<string>();
        List<string> queryResults = GetSQLResults(query);
        if (queryResults.Count != 0)
        {
          if (queryResults[0] != "")
          {
            LogResult.Report(DeviceFirmwarePartNumberSteps.Log, "log_ForInfo", "Expected Value: " + expectedResult.ToString() + ", Actual Value: " + queryResults[0]);
            dbResult = queryResults[0].Equals(expectedResult.ToString());
          }
          if (dbResult == false)
          {
            LogResult.Report(DeviceFirmwarePartNumberSteps.Log, "log_ForError", "DB Verification Failed");
            return false;
          }
        }
        else
        {
          LogResult.Report(DeviceFirmwarePartNumberSteps.Log, "log_ForError", "No Rows Returned From DB");
        }
        return dbResult;
      }
      catch (Exception e)
      {
        LogResult.Report(DeviceFirmwarePartNumberSteps.Log, "log_ForError", "Got error while executing db query", e);
        throw new InvalidDataException("Error Occurred while executing db query");
      }

    }


    public static bool VerifyDeviceFirmware(string deviceUID)
    {
      string DeviceUID = GetUID(deviceUID);
      bool dbResult = true;

      if (DeviceFirmwarePartNumberSteps.IsCellular)
      {
        string query = string.Format(DeviceFirmwareSqlQueries.DeviceFirmwareCellularRadioFirmware, DeviceUID, DeviceFirmwarePartNumberSteps.DeviceFirmwarePartnumberModel.Value);
        dbResult = ValidateDB(query, ExpectedResult);
      }
      if (DeviceFirmwarePartNumberSteps.IsNetwork)
      {
        string query = string.Format(DeviceFirmwareSqlQueries.DeviceFirmwareNetworkFirmware, DeviceUID, DeviceFirmwarePartNumberSteps.DeviceFirmwarePartnumberModel.Value);
        dbResult = ValidateDB(query, ExpectedResult);
      }
      if (DeviceFirmwarePartNumberSteps.IsSatellite)
      {
        string query = string.Format(DeviceFirmwareSqlQueries.DeviceSatelliteRadioNetworkFirmware, DeviceUID, DeviceFirmwarePartNumberSteps.DeviceFirmwarePartnumberModel.Value);
        dbResult = ValidateDB(query, ExpectedResult);
      }

      return dbResult;

    }
    public static string GetUID(string Uid)
    {
      try
      {
        Guid guidResult = Guid.Parse(Uid);
        return guidResult.ToString("N");
      }
      catch (Exception e)
      {
        throw new Exception("Unable To parse guid", e);
      }
    }


  }
}
