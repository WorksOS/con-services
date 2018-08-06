using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV2ForTBCTests
  {
    private static List<TBCPoint> _boundaryLL;

    [TestInitialize]
    public void Initialize()
    {
      _boundaryLL = new List<TBCPoint>()
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };
    }

    [TestMethod]
    public void Create_ProjectV2_All_Ok()
    {
      var msg = new Msg();
      msg.Title("projects 1 V2", "Create a project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();

      var serialized = JsonConvert.SerializeObject(_boundaryLL);
      Assert.AreEqual(@"[{""Latitude"":-43.5,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.603},{""Latitude"":-43.5,""Longitude"":172.603}]", serialized, "TBCPoint not serialized correctly.");

      var response = CreateProjectV2(ts, mysql, "project 1", ProjectType.ProjectMonitoring);
      var createProjectV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);
      
      Assert.AreEqual(HttpStatusCode.Created, createProjectV2Result.Code, "Not created ok.");
      Assert.AreNotEqual(-1, createProjectV2Result.Id, "No Project was created.");
    }

    [TestMethod]
    public void ValidateTBCOrg_All_Ok()
    {
      var msg = new Msg();
      msg.Title("TCM validation V2", "Validate OrgShortName");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();

      // from MockFileRepo in service
      //new Organization()
      //{
      //  filespaceId = "5u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
      //  orgDisplayName = "the orgDisplayName",
      //  orgId = "u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
      //  orgTitle = "the orgTitle",
      //  shortName = "the sn"
      //}

      ts.CreateMockCustomer(ts.CustomerUid, "tbc validate customer", CustomerType.Customer);
      ts.CreateMockCustomerTbcOrgId("u8472cda0-9f59-41c9-a5e2-e19f922f91d8", ts.CustomerUid.ToString());

      var response = ValidateTbcOrgId(ts, mysql, "the sn");
     
      Assert.AreEqual(HttpStatusCode.OK, response.Code, "Not validated ok.");
      Assert.IsTrue( response.Success, "Validation not flagged as successful.");
    }

    private ReturnSuccessV2Result ValidateTbcOrgId(TestSupport ts, MySqlHelper mysql, string orgShortName)
    {
      var response = ts.ValidateTbcOrgIdApiV2(orgShortName, HttpStatusCode.OK);
      Console.WriteLine(response);
      var jsonResponse = JsonConvert.DeserializeObject<ReturnSuccessV2Result>(response);
      return jsonResponse;
    }

    #region privates

    private string CreateProjectV2(TestSupport ts, MySqlHelper mysql, string projectName, ProjectType projectType)
    {
      var response = ts.CreateProjectViaWebApiV2(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, DateTime.UtcNow, _boundaryLL, HttpStatusCode.OK);
      Console.WriteLine(response);

      return response;
    }

    #endregion
  }
}
