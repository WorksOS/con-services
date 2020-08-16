using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TestUtility;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using Xunit;

namespace IntegrationTests.WebApiTests
{
  public class ProjectV5ForTBCTests : WebApiTestsBase
  {
    [Fact]
    public async Task Create_TBCProject_All_Ok()
    {
      Msg.Title("TBC Project", "Create a project");
      var ts = new TestSupport();

      var response = await ts.CreateProjectViaWebApiV5TBC("project 1");
      var returnLongV5Result= JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);
    }

    [Fact]
    public async Task Get_TBCProject_ByCustomerUid_Ok()
    {
      Msg.Title("TBC Project", "Get projects for customer");
      var ts = new TestSupport();

      var projectName = "project 2";
      var projectGeofenceWKT = "POLYGON((172.6 -43.5,172.6 -43.5003,172.603 -43.5003,172.603 -43.5,172.6 -43.5))";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);

      var getResponse = await ts.GetProjectViaWebApiV5TBC();
      var projectDataTbcListResult = JsonConvert.DeserializeObject< Dictionary<long, ProjectDataTBCSingleResult> > (getResponse);

      Assert.NotNull(projectDataTbcListResult);
      Assert.Single(projectDataTbcListResult);
      projectDataTbcListResult.TryGetValue(returnLongV5Result.Id, out var firstProject);
      Assert.NotNull(firstProject);
      Assert.Equal(returnLongV5Result.Id, firstProject.LegacyProjectId);
      Assert.False(firstProject.IsArchived);
      Assert.Equal(projectName, firstProject.Name);
      Assert.Equal(string.Empty, firstProject.ProjectTimeZone);
      Assert.Equal(0, firstProject.ProjectType);                          // only historical 'standard project' supported
      Assert.Equal("Standard", firstProject.ProjectTypeName);
      Assert.Equal(DateTime.MinValue.ToString(), firstProject.StartDate); // no longer supported
      Assert.Equal(DateTime.MaxValue.ToString(), firstProject.EndDate);   // no longer supported
      Assert.True(Guid.TryParse(firstProject.ProjectUid, out _));
      Assert.Equal(projectGeofenceWKT, firstProject.ProjectGeofenceWKT);
      Assert.Equal(returnLongV5Result.Id, firstProject.LegacyProjectId);
      Assert.Equal(ts.CustomerUid, new Guid(firstProject.CustomerUid));
      Assert.Equal("0", firstProject.LegacyCustomerId);               // no longer supported
      Assert.Equal(string.Empty, firstProject.CoordinateSystemFileName);
    }

    [Fact]
    public async Task ValidateTBCOrg_All_Ok()
    {
      Msg.Title("Project V5TBC", "Validate TBCOrg endpoint");
      var ts = new TestSupport();

      var response = await ValidateTbcOrgId(ts, "the sn");

      Assert.Equal(HttpStatusCode.OK, response.Code);
      Assert.True(response.Success, "Validation not flagged as successful.");
    }

    private static async Task<ReturnSuccessV5Result> ValidateTbcOrgId(TestSupport ts, string orgShortName)
    {
      var response = await ts.ValidateTbcOrgIdApiV5(orgShortName);

      return JsonConvert.DeserializeObject<ReturnSuccessV5Result>(response);
    }

  }
}
