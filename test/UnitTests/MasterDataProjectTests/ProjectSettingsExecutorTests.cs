using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.ProjectTests
{
  [TestClass]
  public class ProjectSettingsExecutorTests : ExecutorBaseTests
  {

    [TestMethod]
    public async Task GetProjectSettingsExecutor_NoDataExists()
    {
      string projectUid = Guid.NewGuid().ToString();

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings() { ProjectUid = projectUid, Settings = null };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(projectSettings);

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo.Object, configStore, logger, serviceExceptionHandler, producer.Object );
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.Settings, "executor should have returned empty Settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_DataExists()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings() { ProjectUid = projectUid, Settings = settings };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(projectSettings);

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var executor = RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo.Object, configStore, logger, serviceExceptionHandler, producer.Object);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor should have returned Settings");
    }

    [TestMethod]
    public async Task UpdateProjectSettingsExecutor()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var projectRepo = new Mock<IProjectRepository>();
      var projectSettings = new ProjectSettings() { ProjectUid = projectUid, Settings = settings };
      projectRepo.Setup(ps => ps.GetProjectSettings(It.IsAny<string>())).ReturnsAsync(projectSettings);
      projectRepo.Setup(ps => ps.StoreEvent(It.IsAny<UpdateProjectSettingsEvent>())).ReturnsAsync(1); 

      var configStore = serviceProvider.GetRequiredService<IConfigurationStore>();
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var serviceExceptionHandler = serviceProvider.GetRequiredService<IServiceExceptionHandler>();
      var producer = new Mock<IKafka>();

      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo.Object, configStore, logger, serviceExceptionHandler, producer.Object, kafkaTopicName);
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;

      Assert.IsNotNull(result, "executor failed");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor returned incorrect Settings");
    }

  }
}
