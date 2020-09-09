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
  [Collection("Service collection")]
  public class ProjectSettingsTests
  {
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
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_InvalidCustomerProjectRelationship(ProjectSettingsType settingsType)
    {
      var customerUidOfProject = Guid.NewGuid().ToString();
      var customerUidSomeOther = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = string.Empty;

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUidOfProject);
      Assert.NotNull(project);

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUidOfProject),
          projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () => await executor.ProcessAsync(projectSettingsRequest)).ConfigureAwait(false);
      Assert.NotEqual(-1, ex.GetContent.IndexOf("2001", StringComparison.Ordinal));
      Assert.NotEqual(-1, ex.GetContent.IndexOf("No access to the project for a customer or the project does not exist.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoSettingsExists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUid);
      Assert.NotNull(project);

      var settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
            customerUid, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUid),
            projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
      var projectSettingsResult = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(projectSettingsResult);
      Assert.Equal(project.Id, projectSettingsResult.ProjectUid);
      Assert.Null(projectSettingsResult.Settings);
      Assert.Equal(settingsType, projectSettingsResult.ProjectSettingsType);
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_Exists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var settings = @"{firstValue: 10, lastValue: 20}";

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUid);
      Assert.NotNull(project);

      var isCreatedOk = ExecutorTestFixture.CreateProjectSettings(project.Id, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
          customerUid, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUid),
          projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.Id, result.ProjectUid);
      Assert.Equal(settingsType, result.ProjectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.Equal(tempSettings["firstValue"], result.Settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.Settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.Settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.Settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.Settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.Settings["importedFiles"][1]["lastValue"]);
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

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUidOfProject);
      Assert.NotNull(project);

      var isCreatedOk = ExecutorTestFixture.CreateProjectSettings(project.Id, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUidOfProject),
        productivity3dV2ProxyCompaction: ExecutorTestFixture.Productivity3dV2ProxyCompaction,
        projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
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

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUid);
      Assert.NotNull(project);

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUid),
        productivity3dV2ProxyCompaction: ExecutorTestFixture.Productivity3dV2ProxyCompaction,
        projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.Id, result.ProjectUid);
      Assert.Equal(settingsType, result.ProjectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);

        Assert.Equal(tempSettings["firstValue"], result.Settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.Settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.Settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.Settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.Settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.Settings["importedFiles"][1]["lastValue"]);
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

      var project = await ExecutorTestFixture.CreateCustomerProject(customerUid);
      Assert.NotNull(project);

      var isCreatedOk = ExecutorTestFixture.CreateProjectSettings(project.Id, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(project.Id, settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (ExecutorTestFixture.Logger, ExecutorTestFixture.ConfigStore, ExecutorTestFixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, ExecutorTestFixture.CustomHeaders(customerUid),
        productivity3dV2ProxyCompaction: ExecutorTestFixture.Productivity3dV2ProxyCompaction,
        projectRepo: ExecutorTestFixture.ProjectRepo, cwsProjectClient: ExecutorTestFixture.CwsProjectClient);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(project.Id, result.ProjectUid);
      Assert.Equal(settingsType, result.ProjectSettingsType);

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settingsUpdated);

        Assert.Equal(tempSettings["firstValue"], result.Settings["firstValue"]);
        Assert.Equal(tempSettings["lastValue"], result.Settings["lastValue"]);
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settingsUpdated);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.Equal(tempJObject["importedFiles"][0]["firstValue"], result.Settings["importedFiles"][0]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][0]["lastValue"], result.Settings["importedFiles"][0]["lastValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["firstValue"], result.Settings["importedFiles"][1]["firstValue"]);
        Assert.Equal(tempJObject["importedFiles"][1]["lastValue"], result.Settings["importedFiles"][1]["lastValue"]);
      }
    }
  }
}
