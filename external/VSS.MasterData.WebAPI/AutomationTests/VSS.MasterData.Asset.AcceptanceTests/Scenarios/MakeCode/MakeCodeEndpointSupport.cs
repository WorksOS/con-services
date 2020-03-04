using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationCore.API.Framework.Library;
using VSS.MasterData.Asset.AcceptanceTests.Utils.Config;
using VSS.MasterData.Asset.AcceptanceTests.Models;
using MySql.Data.MySqlClient;
using VSS.MasterData.Asset.AcceptanceTests.Scenarios.MakeCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSS.MasterData.Asset.AcceptanceTests.Scenarios.MakeCode
{
  class MakeCodeEndpointSupport
  {
    public static List<string> VLConnectDBDetails;
    public static string connectionstring;
    public static string result;
    public static List<MakeCodeResponse> MakeCodeDBResult=new List<MakeCodeResponse>();
    
    public static void FetchMakeCodeMsgDetailsFromDB(string query)
    {
      connectionstring = AssetServiceConfig.MySqlConnection;
      ValidateMakeCodeDetails(connectionstring, query);
      //result = MySqlUtil.ExecuteMySQLQueryResult(connectionstring,query);
      //for (int i = 0; i < VLConnectSteps.VLConnectMessageDetails.Count; i++)
      //{
      //  Assert.AreEqual(VLConnectSteps.VLConnectMessageDetails[i], VLConnectDBDetails[i]);
      //}
    }

    public static void ValidateMakeCodeDetails(string connectionString, string query)
    {
      //int i = 0;
      bool result = true;
      MySqlConnection mySqlConnection = new MySqlConnection(connectionString);
      MySqlCommand mySqlCommand;
      mySqlConnection.Open();
      mySqlCommand = new MySqlCommand(query, mySqlConnection);

      using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
      {
        while (reader.Read())
        {
          MakeCodeResponse response = new MakeCodeResponse();
          response.Code = reader["Code"].ToString();
          response.Name = reader["Name"].ToString();
          MakeCodeDBResult.Add(response);

        }
      }




      //var difference = MakeCodeDBResult
      //        .Select(m1 => new { m1.Code, m1.Name })
      //        .Except(MakeCodeEndpointSteps.MakeCodeAPIResponse
      //        .Select(m2 => new { m2.Code, m2.Name }));

      for (int i = 0; i < MakeCodeDBResult.Count; i++)
      {
        if ((MakeCodeDBResult[i].Code != MakeCodeEndpointSteps.MakeCodeAPIResponse[i].Code) || (MakeCodeDBResult[i].Name != MakeCodeEndpointSteps.MakeCodeAPIResponse[i].Name))
        {
          result = false;
          break;
        }
      }
      Assert.AreEqual(result,true);
      
    }
    public static string GetUID(string Uid)
    {
      Guid guidResult = new Guid();
      try
      {
        guidResult = Guid.Parse(Uid);
        return guidResult.ToString("N");
      }
      catch (Exception e)
      {
        throw new Exception("Unable To parse guid", e);
      }
    }

  }
}
