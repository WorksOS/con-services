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
using VSS.MasterData.Customer.AcceptanceTests.Utils.Config;
using VSS.MasterData.Customer.AcceptanceTests.Utils.Features.Classes.AccountHierarchy;

namespace VSS.MasterData.Customer.AcceptanceTests.Scenarios.AccountHierarchy
{
  class AccountHierarchyServiceSupport
  {
    private static Log4Net Log = new Log4Net(typeof(AccountHierarchyServiceSupport));
    #region AccountHierarchyServiceSupport

    public AccountHierarchyServiceSupport(Log4Net myLog)
    {
      CustomerServiceConfig.SetupEnvironment();
      Log = myLog;
    }

    #endregion

    #region DB Methods

    public static bool ValidateDB(AccountHierarchyDBComparisonClass AccountHierarchyNode)
    {
     List<AccountHierarchyDBComparisonClass> AccountHierarchyQueryResult = new List<AccountHierarchyDBComparisonClass>();
        //WaitForDB();
        bool dbResult = false;
      string query = string.Empty;
      int expectedResult = 1;
      string nodeUID = AccountHierarchyNode.NodeUID.ToString().Replace("-", "");
      string rootNodeUID = AccountHierarchyNode.RootNodeUID.ToString().Replace("-", "");
      string parentNodeUID = AccountHierarchyNode.ParentNodeUID.ToString().Replace("-", "");

      if (expectedResult == 1)
      {
         query = string.Format(DBQueries.AccountHierarchyValidation, rootNodeUID, parentNodeUID, nodeUID,AccountHierarchyNode.LeftPosition.ToString(),AccountHierarchyNode.RightPosition.ToString());
      }
      else
      {
         query = string.Format(DBQueries.AccountHierarchyCountByRootNodeUID, AccountHierarchyNode.RootNodeUID);
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

    public static bool ValidateDB( int expectedResult)
    {
      List<AccountHierarchyDBComparisonClass> AccountHierarchyQueryResult = new List<AccountHierarchyDBComparisonClass>();
      //WaitForDB();
      bool dbResult = false;
      string query = string.Empty;
      //int expectedResult = 1;
      string rootNodeUID = AccountHierarchyServiceSteps.parentUID[0].ToString().Replace("-","");
        query = string.Format(DBQueries.AccountHierarchyCountByRootNodeUID, rootNodeUID);
      
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

    public static List<string> GetSQLResults(string queryString)
    {
      MySqlDataReader dataReader = null;
      List<string> dbResult = new List<string>();
      using (MySqlConnection mySqlConnection = new MySqlConnection(CustomerServiceConfig.MySqlConnection))
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

    public string PostValidReadRequestToService(string accessToken)
    {
      try
      {
        LogResult.Report(Log, "log_ForInfo", "Reading the list of available geofences for the user accesstoken the request with Valid Values: " + accessToken);
        string ResponseString = RestClientUtil.DoHttpRequest(CustomerServiceConfig.CustomerServiceEndpoint + "/accounthierarchy", HeaderSettings.GetMethod, accessToken,
          HeaderSettings.JsonMediaType, null, HttpStatusCode.OK, HeaderSettings.BearerType, HeaderSettings.JsonMediaType);
        return ResponseString;
      }
      catch (Exception e)
      {
        LogResult.Report(Log, "log_ForError", "Got Error While Posting Data To Geofence Service", e);
        throw new Exception(e + " Got Error While Posting Data To Geofence Service");
      }
    }
  }
}
