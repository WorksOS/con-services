using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectSettingsExecutorTests : ExecutorBaseTests
  {
    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_NoDataExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string userId = Guid.NewGuid().ToString();
      string userEmailAddress = "whatever@here.there.com";
      string projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), settingsType)).ReturnsAsync((ProjectSettings)null);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, userEmailAddress, null,
        null, null,
        null, null, null, null, null,
        projectRepo.Object );
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      
      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNull(result.settings, "executor should have returned empty settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, $"executor should have returned {settingsType} projectSettingsType");
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task GetProjectSettingsExecutor_DataExists(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings = string.Empty;
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);

      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        null, null,
        null, null, null, null, null,
        projectRepo.Object );
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNull(result.settings, "executor should have returned null settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_MultipleSettings()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings1 = string.Empty;
      string settings2 = @"{firstValue: 10, lastValue: 20}";
      string userId = "my app";
      ProjectSettingsType settingsType1 = ProjectSettingsType.ImportedFiles;
      ProjectSettingsType settingsType2 = ProjectSettingsType.Targets;

      var projectRepo = new Mock<IProjectRepository>();
      var project = new Repositories.DBModels.Project() { CustomerUID = customerUid, ProjectUID = projectUid };
      var projectList = new List<Repositories.DBModels.Project>(); projectList.Add(project);
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
        customerUid, userId, null, null,
        null, null,
        null, null, null, null, null,
        projectRepo.Object);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings2);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNotNull(result.settings, "executor should have returned settings");
      Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"], "executor returned incorrect firstValue of settings");
      Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"], "executor should have returned lastValue of settings");
      Assert.AreEqual(settingsType2, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_ProjectCustomerValidationFails()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var projectList = new List<Repositories.DBModels.Project>(); 
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ProjectSettingsType>())).ReturnsAsync((ProjectSettings)null);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var projectErrorCodesProvider = ServiceProvider.GetRequiredService<IErrorCodesProvider>();

      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, string.Empty, ProjectSettingsType.Targets);

      var executor = RequestExecutorContainerFactory.Build<GetProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
          customerUid, userId, null, null,
          null, null,
          null, null, null, null, null,
          projectRepo.Object);
      var ex = await Assert.ThrowsExceptionAsync<ServiceException>( async () =>
        await executor.ProcessAsync(projectSettingsRequest));

      Assert.AreNotEqual(-1, ex.GetContent.IndexOf(projectErrorCodesProvider.FirstNameWithOffset(1)));
    }

    [TestMethod]
    [DataRow(ProjectSettingsType.Targets)]
    [DataRow(ProjectSettingsType.ImportedFiles)]
    [DataRow(ProjectSettingsType.Colors)]
    public async Task UpsertProjectSettingsExecutor(ProjectSettingsType settingsType)
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();

      string settings = settingsType != ProjectSettingsType.ImportedFiles ? @"{firstValue: 10, lastValue: 20}" : @"[{firstValue: 10, lastValue: 20}, {firstValue: 20, lastValue: 40}]";
      
      string userId = "my app";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings { ProjectUid = projectUid, Settings = settings, ProjectSettingsType = settingsType, UserID = userId};
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType)).ReturnsAsync(projectSettings);
      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(new Repositories.DBModels.Project(){ ProjectUID = projectUid});
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1); 

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new BaseDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
        (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        producer.Object, KafkaTopicName,
        raptorProxy.Object, null, null, null, null,
        projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, settingsType);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNotNull(result.settings, "executor should have returned settings");
      Assert.AreEqual(settingsType, result.projectSettingsType, "executor returned incorrect projectSettingsType");

      if (settingsType == ProjectSettingsType.Targets || settingsType == ProjectSettingsType.Colors)
      {
        var tempSettings = JsonConvert.DeserializeObject<JObject>(settings);
        
        Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"],
          "executor returned incorrect firstValue of settings");
        Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"],
          "executor should have returned lastValue of settings");
      }
      else
      {
        var tempObj = JsonConvert.DeserializeObject<JArray>(settings);
        var tempJObject = new JObject { ["importedFiles"] = tempObj };

        Assert.AreEqual(tempJObject["importedFiles"][0]["firstValue"], result.settings["importedFiles"][0]["firstValue"], "executor returned incorrect firstValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][0]["lastValue"], result.settings["importedFiles"][0]["lastValue"], "executor returned incorrect lastValue of the first object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["firstValue"], result.settings["importedFiles"][1]["firstValue"], "executor returned incorrect firstValue of the last object of the settings");
        Assert.AreEqual(tempJObject["importedFiles"][1]["lastValue"], result.settings["importedFiles"][1]["lastValue"], "executor returned incorrect lastValue of the last object of the settings");
      }
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_MultipleSettings()
    {
      string customerUid = Guid.NewGuid().ToString();
      string projectUid = Guid.NewGuid().ToString();
      string settings1 = @"{firstValue: 10, lastValue: 20}";
      string settings2 = @"{firstValue: 30, lastValue: 40}";

      string userId = "my app";
      ProjectSettingsType settingsType1 = ProjectSettingsType.Targets;
      ProjectSettingsType settingsType2 = ProjectSettingsType.ImportedFiles;

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings1 = new ProjectSettings { ProjectUid = projectUid, Settings = settings1, ProjectSettingsType = settingsType1, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType1)).ReturnsAsync(projectSettings1);
      var projectSettings2 = new ProjectSettings { ProjectUid = projectUid, Settings = settings2, ProjectSettingsType = settingsType2, UserID = userId };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>(), userId, settingsType2)).ReturnsAsync(projectSettings2);

      var projectList = new List<Repositories.DBModels.Project>();
      projectList.Add(new Repositories.DBModels.Project() { ProjectUID = projectUid });
      projectRepo.Setup(ps => ps.GetProjectsForCustomer(It.IsAny<string>())).ReturnsAsync(projectList);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1);

      var configStore = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var logger = ServiceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = ServiceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();
      producer.Setup(p => p.InitProducer(It.IsAny<IConfigurationStore>()));
      producer.Setup(p => p.Send(It.IsAny<string>(), It.IsAny<List<KeyValuePair<string, string>>>()));

      var raptorProxy = new Mock<IRaptorProxy>();
      raptorProxy.Setup(r => r.ValidateProjectSettings(It.IsAny<ProjectSettingsRequest>(),
        It.IsAny<IDictionary<string, string>>())).ReturnsAsync(new BaseDataResult());

      var executor = RequestExecutorContainerFactory.Build<UpsertProjectSettingsExecutor>
      (logger, configStore, serviceExceptionHandler,
        customerUid, userId, null, null,
        producer.Object, KafkaTopicName,
        raptorProxy.Object, null, null, null, null,
        projectRepo.Object);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings1, settingsType1);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      var tempSettings = JsonConvert.DeserializeObject<JObject>(settings1);

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.projectUid, "executor returned incorrect projectUid");
      Assert.IsNotNull(result.settings, "executor should have returned settings");
      Assert.AreEqual(tempSettings["firstValue"], result.settings["firstValue"], "executor returned incorrect firstValue of settings");
      Assert.AreEqual(tempSettings["lastValue"], result.settings["lastValue"], "executor should have returned lastValue of settings");
      Assert.AreEqual(settingsType1, result.projectSettingsType, "executor returned incorrect projectSettingsType");
    }

    [TestMethod]
    public void ProjectSettingsRequestShouldNotSerializeType()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var request = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(request);
      Assert.IsFalse(json.Contains("ProjectSettingsType"));
    }

    [TestMethod]
    public void ProjectSettingsResultShouldNotSerializeType()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = @"{firstValue: 10, lastValue: 20}";

      var result = ProjectSettingsResult.CreateProjectSettingsResult(projectUid, JsonConvert.DeserializeObject<JObject>(settings), ProjectSettingsType.Targets);
      var json = JsonConvert.SerializeObject(result);
      Assert.IsFalse(json.Contains("ProjectSettingsType"));
    }



  }
}
