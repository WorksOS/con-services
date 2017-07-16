using System;
using System.Threading.Tasks;
using KafkaConsumer.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.ProjectWebApiCommon.Executors;
using Microsoft.Extensions.Logging;
using VSS.GenericConfiguration;
using VSS.Productivity3D.ProjectWebApiCommon.Internal;
using VSS.Productivity3D.ProjectWebApiCommon.Models;
using VSS.Productivity3D.ProjectWebApiCommon.ResultsHandling;
using Moq;
using VSS.Productivity3D.Repo;
using VSS.Productivity3D.Repo.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectTests
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
