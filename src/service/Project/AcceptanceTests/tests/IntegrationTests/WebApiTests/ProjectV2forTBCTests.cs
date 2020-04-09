﻿using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestUtility;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV2ForTBCTests : WebApiTestsBase
  {
    private static List<TBCPoint> _boundaryLL;

    public ProjectV2ForTBCTests()
    {
      _boundaryLL = new List<TBCPoint>
      {
        new TBCPoint(-43.5, 172.6),
        new TBCPoint(-43.5003, 172.6),
        new TBCPoint(-43.5003, 172.603),
        new TBCPoint(-43.5, 172.603)
      };
    }

    [Fact]
    public async Task Create_ProjectV2_All_Ok()
    {
      Msg.Title("projects 1 V2", "Create a project");
      var ts = new TestSupport();

      var serialized = JsonConvert.SerializeObject(_boundaryLL);
      Assert.Equal(@"[{""Latitude"":-43.5,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.6},{""Latitude"":-43.5003,""Longitude"":172.603},{""Latitude"":-43.5,""Longitude"":172.603}]", serialized);

      var response = await CreateProjectV2(ts, "project 1", ProjectType.ProjectMonitoring);
      var createProjectV2Result = JsonConvert.DeserializeObject<ReturnLongV2Result>(response);
      
      Assert.Equal(HttpStatusCode.Created, createProjectV2Result.Code);
      Assert.NotEqual(-1, createProjectV2Result.Id);
    }

    [Fact]
    public async Task ValidateTBCOrg_All_Ok()
    {
      Msg.Title("TCM validation V2", "Validate OrgShortName");
      var ts = new TestSupport();

      ts.CreateMockCustomer(ts.CustomerUid, "tbc validate customer", CustomerType.Customer);
      ts.CreateMockCustomerTbcOrgId("u8472cda0-9f59-41c9-a5e2-e19f922f91d8", ts.CustomerUid.ToString());

      var response = await ValidateTbcOrgId(ts, "the sn");
     
      Assert.Equal(HttpStatusCode.OK, response.Code);
      Assert.True( response.Success, "Validation not flagged as successful.");
    }

    private static async Task<ReturnSuccessV2Result> ValidateTbcOrgId(TestSupport ts, string orgShortName)
    {
      var response = await ts.ValidateTbcOrgIdApiV2(orgShortName);

      return JsonConvert.DeserializeObject<ReturnSuccessV2Result>(response);
    }

    private static Task<string> CreateProjectV2(TestSupport ts, string projectName, ProjectType projectType)
    {
      return ts.CreateProjectViaWebApiV2(projectName, ts.FirstEventDate, ts.LastEventDate, "New Zealand Standard Time", projectType, _boundaryLL);
    }
  }
}