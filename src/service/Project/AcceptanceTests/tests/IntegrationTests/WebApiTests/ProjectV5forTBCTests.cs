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
    public async Task Get_TBCProject_ByProjectId_Ok()
    {
      Msg.Title("TBC Project", "Get existing project");
      var ts = new TestSupport();

      var projectName = "project 2";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);

      var getResponse = await ts.GetProjectViaWebApiV5TBC(returnLongV5Result.Id);
      var projectDataTbcSingleResult = JsonConvert.DeserializeObject<ProjectDataTBCSingleResult>(getResponse);

      Assert.NotNull(projectDataTbcSingleResult);
      Assert.Equal(returnLongV5Result.Id, projectDataTbcSingleResult.LegacyProjectId);
      Assert.Equal(DateTime.MinValue.ToString(), projectDataTbcSingleResult.StartDate); // no longer supported
      Assert.Equal(DateTime.MaxValue.ToString(), projectDataTbcSingleResult.EndDate);  // no longer supported
      Assert.Equal(projectName, projectDataTbcSingleResult.Name);
      Assert.Equal(0, projectDataTbcSingleResult.ProjectType); // only historical 'standard project' supported
    }

    [Fact]
    public async Task Get_TBCProject_ByCustomerUid_Ok()
    {
      Msg.Title("TBC Project", "Get projects for customer");
      var ts = new TestSupport();

      var projectName = "project 3";
      var createResponse = await ts.CreateProjectViaWebApiV5TBC(projectName);
      var returnLongV5Result = JsonConvert.DeserializeObject<ReturnLongV5Result>(createResponse);

      Assert.Equal(HttpStatusCode.Created, returnLongV5Result.Code);
      Assert.NotEqual(-1, returnLongV5Result.Id);

      var getResponse = await ts.GetProjectViaWebApiV5TBC();
      var projectDataTbcListResult = JsonConvert.DeserializeObject<ProjectDataTBCListResult>(getResponse);

      Assert.NotNull(projectDataTbcListResult);
      Assert.Single(projectDataTbcListResult.ProjectDescriptors);
      Assert.Equal(returnLongV5Result.Id, projectDataTbcListResult.ProjectDescriptors[0].LegacyProjectId);
      Assert.Equal(DateTime.MinValue.ToString(), projectDataTbcListResult.ProjectDescriptors[0].StartDate); // no longer supported
      Assert.Equal(DateTime.MaxValue.ToString(), projectDataTbcListResult.ProjectDescriptors[0].EndDate);  // no longer supported
      Assert.Equal(projectName, projectDataTbcListResult.ProjectDescriptors[0].Name);
      Assert.Equal(0, projectDataTbcListResult.ProjectDescriptors[0].ProjectType); // only historical 'standard project' supported
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
