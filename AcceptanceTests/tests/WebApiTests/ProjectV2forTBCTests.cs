using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
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
      CreateMockSubscription(ts, ServiceTypeEnum.ProjectMonitoring, startDate, endDate);

      var projectId = CreateProjectV2(ts, mysql, "project 1", ProjectType.ProjectMonitoring);
      Assert.AreNotEqual(-1, projectId, "No Project was created.");
    }

    #region privates

    private long CreateProjectV2(TestSupport ts, MySqlHelper mysql, string projectName, ProjectType projectType)
    {
      long projectId = -1;
      var response = ts.CreateProjectViaWebApiV2(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, DateTime.UtcNow, PROJECT_BOUNDARY, HttpStatusCode.OK);
      Console.WriteLine(response);
      var jsonResponse = JsonConvert.DeserializeObject<CreateProjectV2Result>(response);
      if (jsonResponse.Code == 0)
      {
        projectId = jsonResponse.projectId;
      }

      return projectId;
    }
    private void CreateMockSubscription(TestSupport ts, ServiceTypeEnum serviceTypeEnum, DateTime subStartDate, DateTime subEndDate)
    {
      ts.CreateMockSubscription(serviceTypeEnum, ts.SubscriptionUid.ToString(), ts.CustomerUid.ToString(), subStartDate, subEndDate);
    }

    #endregion
  }
}
