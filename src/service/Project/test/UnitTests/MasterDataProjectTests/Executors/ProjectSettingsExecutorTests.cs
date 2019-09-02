using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.ProjectTests.Executors
{
  public class ProjectSettingsExecutorTestsDiFixture : UnitTestsDIFixture<ProjectSettingsExecutorTestsDiFixture>
  {
    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoDataExists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var userId = Guid.NewGuid().ToString();
      var userEmailAddress = "whatever@here.there.com";
      var projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), settingsType)).ReturnsAsync((ProjectSettings)null);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.Null(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_DataExists(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var settings = string.Empty;
      var userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.Null(result.settings);
      Assert.Equal(settingsType, result.projectSettingsType);
    }

    [Fact]
    public async Task GetProjectSettingsExecutor_MultipleSettings()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var settings1 = string.Empty;
      var settings2 = @"{firstValue: 10, lastValue: 20}";
      var userId = "my app";
      var settingsType1 = ProjectSettingsType.ImportedFiles;
      var settingsType2 = ProjectSettingsType.Targets;

      var projectRepo = new Mock<IProjectRepository>();
      var project = new ProjectDatabaseModel { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<ProjectDatabaseModel> { project };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var projectSettings1 = new ProjectSettings { ProjectUid = projectUid, Settings = settings1, ProjectSettingsType = settingsType1, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType1)).ReturnsAsync(projectSettings1);
      var projectSettings2 = new ProjectSettings { ProjectUid = projectUid, Settings = settings2, ProjectSettingsType = settingsType2, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType2)).ReturnsAsync(projectSettings2);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings2, settingsType2);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, projectRepo: projectRepo.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings2);

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.NotNull(result.settings);
      Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
      Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      Assert.Equal(settingsType2, result.projectSettingsType);
    }

    [Fact]
    public async Task GetProjectSettingsExecutor_ProjectCustomerValidationFails()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(new List<ProjectDatabaseModel>());
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProjectSettingsType>())).ReturnsAsync((ProjectSettings)null);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, ProjectSettingsType.Targets);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, projectRepo: projectRepo.Object);
      var ex = await Assert.ThrowsAsync<ServiceException>(async () =>
       await executor.ProcessAsync(projectSettingsRequest));

      Assert.NotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(1)));
    }

    [Theory]
    [InlineData(ProjectSettingsType.Targets)]
    [InlineData(ProjectSettingsType.ImportedFiles)]
    [InlineData(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor(ProjectSettingsType settingsType)
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();

      var settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";

      var userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);
      var projectList = new List<ProjectDatabaseModel> { new ProjectDatabaseModel { ProjectUID = projectUid } };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));

      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new BaseMasterDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        producer.Object, KafkaTopicName,
        productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object, projectRepo: projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.NotNull(result.settings);
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

    [Fact]
    public async Task UpsertProjectSettingsExecutor_MultipleSettings()
    {
      var customerUid = Guid.NewGuid().ToString();
      var projectUid = Guid.NewGuid().ToString();
      var settings1 = @"{firstValue: 10, lastValue: 20}";
      var settings2 = @"{firstValue: 30, lastValue: 40}";

      var userId = "my app";
      var settingsType1 = ProjectSettingsType.Targets;
      var settingsType2 = ProjectSettingsType.ImportedFiles;

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings1 = new ProjectSettings { ProjectUid = projectUid, Settings = settings1, ProjectSettingsType = settingsType1, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType1)).ReturnsAsync(projectSettings1);
      var projectSettings2 = new ProjectSettings { ProjectUid = projectUid, Settings = settings2, ProjectSettingsType = settingsType2, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType2)).ReturnsAsync(projectSettings2);

      var projectList = new List<ProjectDatabaseModel> { new ProjectDatabaseModel { ProjectUID = projectUid } };
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var productivity3dV2ProxyCompaction = new Mock<IProductivity3dV2ProxyCompaction>();
      productivity3dV2ProxyCompaction.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new BaseMasterDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        producer.Object, KafkaTopicName,
        productivity3dV2ProxyCompaction: productivity3dV2ProxyCompaction.Object, projectRepo: projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings1, settingsType1);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings1);

      Assert.NotNull(result);
      Assert.Equal(projectUid, result.projectUid);
      Assert.NotNull(result.settings);
      Assert.Equal(tempSettings["firstValue"], result.settings["firstValue"]);
      Assert.Equal(tempSettings["lastValue"], result.settings["lastValue"]);
      Assert.Equal(settingsType1, result.projectSettingsType);
    }

    [Fact]
    public void ProjectSettingsRequestShouldNotSerializeType()
    {
      var projectUid = Guid.NewGuid().ToString();
      var settings = "blah";

      var request = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(request);
      Assert.False(json.Contains("ProjectSettingsType"));
    }

    [Fact]
    public void ProjectSettingsResultShouldNotSerializeType()
    {
      var projectUid = Guid.NewGuid().ToString();
      var settings = @"{firstValue: 10, lastValue: 20}";

      var result = ProjectSettingsResult.CreateProjectSettingsResult(projectUid, JsonConvert.DeserializeObject<JObject>(settings), ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(result);
      Assert.False(json.Contains("ProjectSettingsType"));
    }
  }
}
