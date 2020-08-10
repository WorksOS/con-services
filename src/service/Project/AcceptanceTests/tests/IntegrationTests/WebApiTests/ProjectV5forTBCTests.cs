using System;
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
    [Fact(Skip = "Waiting for CCSSSCON-396")]
    public async Task Create_TBCProject_All_Ok()
    {
      Msg.Title("TBC Project", "Create a project");
      var ts = new TestSupport();

      var response = await ts.CreateProjectViaWebApiV5TBC("project 1");
      var returnLongV5Result= JsonConvert.DeserializeObject<ReturnLongV5Result>(response);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);
    }

    [Fact(Skip = "Waiting for CCSSSCON-396")]
    public async Task Get_TBCProject_All_Ok()
    {
      Msg.Title("TBC Project", "Get existing project");
      var ts = new TestSupport();

      var projectName = "project 2";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);

      var getResponse = await ts.GetProjectViaWebApiV5TBC(returnLongV5Result.Id);
      var projectDataTBCSingleResult = JsonConvert.DeserializeObject<ProjectDataTBCSingleResult>(getResponse);

      Assert.NotNull(projectDataTBCSingleResult);
      Assert.Equal(returnLongV5Result.Id, projectDataTBCSingleResult.LegacyProjectId);
      Assert.Equal(DateTime.MinValue.ToString(), projectDataTBCSingleResult.StartDate); // no longer supported
      Assert.Equal(DateTime.MaxValue.ToString(), projectDataTBCSingleResult.EndDate);  // no longer supported
      Assert.Equal(projectName, projectDataTBCSingleResult.Name);
      Assert.Equal(0, projectDataTBCSingleResult.ProjectType); // only historical standard supported
    }

    [Fact(Skip = "Waiting for CCSSSCON-396")]
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
