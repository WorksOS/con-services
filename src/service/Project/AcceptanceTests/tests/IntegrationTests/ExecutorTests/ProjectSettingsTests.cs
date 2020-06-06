using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace IntegrationTests.ExecutorTests
{
  public class ProjectSettingsTests : IClassFixture<ExecutorTestFixture>
  {
    private readonly ExecutorTestFixture _fixture;
    public ProjectSettingsTests(ExecutorTestFixture fixture)
    {
      _fixture = fixture;
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    public void GetProjectSettingsExecutor_InvalidProjectUid(ProjectSettingsType settingsType)
    {
      var projectUid = string.Empty;
      var settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var ex = Assert.Throws<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2005", StringComparison.Ordinal));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("Missing ProjectUID.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    //[InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      var customerUidOfProject = Guid.NewGuid().ToString();
      var customerUidSomeOther = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = string.Empty;

      var project = await _fixture.CreateCustomerProject(customerUidOfProject, userId, userEmailAddress);
      Assert.NotNull(project);

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, _fixture.CustomHeaders(customerUidOfProject),
          projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2001", StringComparison.Ordinal));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    //[InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoSettingsExists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";

      var project = await _fixture.CreateCustomerProject(customerUid, userId, userEmailAddress);
      Assert.NotNull(project);

      var settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
            customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
            projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var projectSettingsResult = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(projectSettingsResult);
      Assert.Equal(project.ProjectDescriptor.ProjectUid, projectSettingsResult.projectUid);
      Assert.Null(projectSettingsResult.settings);
      Assert.Equal(settingsType, projectSettingsResult.projectSettingsType);
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    //[InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = @"{firstValue: 10, lastValue: 20}";

      var project = await _fixture.CreateCustomerProject(customerUid, userId, userEmailAddress);
      Assert.NotNull(project);

      var isCreatedOk = _fixture.CreateProjectSettings(project.ProjectDescriptor.ProjectUid, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
          customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
          projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.ProjectDescriptor.ProjectUid, result.projectUid);
      Assert.Equal(settingsType, result.projectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"]);
      }
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    public void UpsertProjectSettingsExecutor_InvalidProjectSettings(ProjectSettingsType settingsType)
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, null, settingsType);
      var ex = Assert.Throws<ServiceException>(() => projectSettingsRequest.Validate());
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2073", StringComparison.Ordinal));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("ProjectSettings cannot be null.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    public async Task UpsertProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      var customerUidOfProject = Guid.NewGuid().ToString();
      var customerUidSomeOther = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = "blah";
      var settingsUpdated = "blah Is Updated";

      var project = await _fixture.CreateCustomerProject(customerUidOfProject, userId, userEmailAddress);
      Assert.NotNull(project);

      var isCreatedOk = _fixture.CreateProjectSettings(project.ProjectDescriptor.ProjectUid, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, _fixture.CustomHeaders(customerUidOfProject),
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction,
        projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2001", StringComparison.Ordinal));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor_NoProjectSettingsExists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";//"blah";

      var project = await _fixture.CreateCustomerProject(customerUid, userId, userEmailAddress);
      Assert.NotNull(project);

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction,
        projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.ProjectDescriptor.ProjectUid, result.projectUid);
      Assert.Equal(settingsType, result.projectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"]);
      }
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = "blah";
      var settingsUpdated = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";

      var project = await _fixture.CreateCustomerProject(customerUid, userId, userEmailAddress);
      Assert.NotNull(project);

      var isCreatedOk = _fixture.CreateProjectSettings(project.ProjectDescriptor.ProjectUid, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(project.ProjectDescriptor.ProjectUid, settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction,
        projectRepo: _fixture.ProjectRepo, cwsProjectClient: _fixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.ProjectDescriptor.ProjectUid, result.projectUid);
      Assert.Equal(settingsType, result.projectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settingsUpdated);

        Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settingsUpdated);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"]);
      }
    }
  }
}
