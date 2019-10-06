using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
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
      var projectUid = Guid.NewGuid().ToString();
      var settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = _fixture.CreateCustomerProject(customerUidOfProject, projectUid);
      Assert.True(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
          customerUidSomeOther, userId, userEmailAddress, _fixture.CustomHeaders(customerUidOfProject),
          projectRepo: _fixture.ProjectRepo);
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
      var projectUid = Guid.NewGuid().ToString();
      var settings = string.Empty;
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var isCreatedOk = _fixture.CreateCustomerProject(customerUid, projectUid);
      Assert.True(isCreatedOk, "unable to create project for Customer");

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
          (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
            customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
            projectRepo: _fixture.ProjectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.Null(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);
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
      var projectUid = Guid.NewGuid().ToString();
      var settings = @"{firstValue: 10, lastValue: 20}";//"blah";

      var isCreatedOk = _fixture.CreateCustomerProject(customerUid, projectUid);
      Assert.True(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = _fixture.CreateProjectSettings(projectUid, userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor =
        RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
          customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
          projectRepo: _fixture.ProjectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
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
      var projectUid = Guid.NewGuid();
      var settings = "blah";
      var settingsUpdated = "blah Is Updated";

      var isCreatedOk = _fixture.CreateCustomerProject(customerUidOfProject, projectUid.ToString());
      Assert.True(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = _fixture.CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUidSomeOther, userId, userEmailAddress, _fixture.CustomHeaders(customerUidOfProject),
        _fixture.Producer, _fixture.KafkaTopicName,
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction, projectRepo: _fixture.ProjectRepo);
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
      var projectUid = Guid.NewGuid();
      var settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";//"blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings, settingsType);

      var isCreatedOk = _fixture.CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.True(isCreatedOk, "unable to create project for Customer");

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
        _fixture.Producer, _fixture.KafkaTopicName,
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction, projectRepo: _fixture.ProjectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(projectUid.ToString(), result.projectUid);
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
      var projectUid = Guid.NewGuid();
      var settings = "blah";
      //string settingsUpdated = "blah Is Updated";
      var settingsUpdated = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";

      var isCreatedOk = _fixture.CreateCustomerProject(customerUid, projectUid.ToString());
      Assert.True(isCreatedOk, "unable to create project for Customer");

      isCreatedOk = _fixture.CreateProjectSettings(projectUid.ToString(), userId, settings, settingsType);
      Assert.True(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated, settingsType);

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (_fixture.Logger, _fixture.ConfigStore, _fixture.ServiceExceptionHandler,
        customerUid, userId, userEmailAddress, _fixture.CustomHeaders(customerUid),
        _fixture.Producer, _fixture.KafkaTopicName,
        productivity3dV2ProxyCompaction: _fixture.Productivity3dV2ProxyCompaction, projectRepo: _fixture.ProjectRepo);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.NotNull(result);
      Assert.Equal(projectUid.ToString(), result.projectUid);
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
