using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests
{
  [TestClass]
  public class ProjectV2ForTBCTests
  {
    private const string PROJECT_BOUNDARY = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))";

   [TestMethod]
    public void Create_ProjectV2_All_Ok()
    {
      var msg = new Msg();
      msg.Title("projects 1 V2", "Create a project");
      var mysql = new MySqlHelper();
      var ts = new TestSupport();

      DateTime startDate = ts.ConvertTimeStampAndDayOffSetToDateTime("0d+00:00:00", ts.FirstEventDate);
      DateTime endDate = ts.ConvertTimeStampAndDayOffSetToDateTime("10000d+00:00:00", ts.FirstEventDate);
      ts.CreateMockSubscription(ServiceTypeEnum.ProjectMonitoring, ts.SubscriptionUid.ToString(), ts.CustomerUid.ToString(), startDate, endDate);

      var response = CreateProjectV2(ts, mysql, "project 1", ProjectType.ProjectMonitoring);

      var createProjectV2Result = JsonConvert.DeserializeObject<CreateProjectV2Result>(response);
      
      Assert.AreEqual((int)HttpStatusCode.Created, createProjectV2Result.Code, "Not created ok.");
      Assert.AreNotEqual(-1, createProjectV2Result.id, "No Project was created.");
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
     
      Assert.AreEqual((int)HttpStatusCode.OK, response.Code, "Not validated ok.");
      Assert.AreEqual("\"success\":true", response.Message, "Validation message not ok.");
    }

    private ContractExecutionResult ValidateTbcOrgId(TestSupport ts, MySqlHelper mysql, string orgShortName)
    {
      long projectId = -1;
      var response = ts.ValidateTbcOrgIdApiV2(orgShortName, HttpStatusCode.OK);
      Console.WriteLine(response);
      var jsonResponse = JsonConvert.DeserializeObject<ContractExecutionResult>(response);
      return jsonResponse;
    }

    #region privates

    private string CreateProjectV2(TestSupport ts, MySqlHelper mysql, string projectName, ProjectType projectType)
    {
      long projectId = -1;
      var response = ts.CreateProjectViaWebApiV2(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.OK);
      Console.WriteLine(response);

      return response;
    }

    #endregion
  }
}
