using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectSettingsTests : ExecutorTestsBase
  {
    //private ILogger log = logger.CreateLogger<ProjectSettingsTests>();

    [TestMethod]
    public async Task GetProjectSettingsExecutor_NoneExists()
    {
      string projectUid = Guid.NewGuid().ToString();

      var executor =
        RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger,
          serviceExceptionHandler, producer);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.IsNull(result.Settings, "executor should have returned null Settings");
    }

    [TestMethod]
    public async Task GetProjectSettingsExecutor_Exists()
    {
      string projectUid = Guid.NewGuid().ToString();
      string settings = "blah";

      var isCreatedOk = CreateProjectSettings(projectUid, settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var executor =
        RequestExecutorContainer.Build<GetProjectSettingsExecutor>(projectRepo, configStore, logger,
          serviceExceptionHandler, producer);
      var result = await executor.ProcessAsync(projectUid) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_NoneExists()
    {
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var projectSettingsRequest = ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      string kafkaTopicName = "VSS.Interfaces.Events.MasterData.IProjectEvent" +
                              configStore.GetValueString("KAFKA_TOPIC_NAME_SUFFIX");

      var executor = RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, configStore, logger,
        serviceExceptionHandler, producer, kafkaTopicName);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settings, result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task UpsertProjectSettingsExecutor_Exists()
    {
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      string settingsUpdated = "blah Is Updated";

      var isCreatedOk = CreateProjectSettings(projectUid.ToString(), settings);
      Assert.IsTrue(isCreatedOk, "created projectSettings");

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settingsUpdated);
      var executor =
        RequestExecutorContainer.Build<UpsertProjectSettingsExecutor>(projectRepo, configStore, logger,
          serviceExceptionHandler, producer);
      var result = await executor.ProcessAsync(projectSettingsRequest) as ProjectSettingsResult;
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(settingsUpdated, result.Settings, "executor returned incorrect Settings");
    }

    [TestMethod]
    public async Task ValidateProjectSettings_Success()
    {
      var projectUid = Guid.NewGuid();
      string settings = "blah";
      var log = logger.CreateLogger<ProjectSettingsTests>();

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      var result = await ProjectSettingsValidation.RaptorValidateProjectSettings(raptorProxy, log,
        serviceExceptionHandler, projectSettingsRequest, customHeaders);

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(200, result.Code, "executor returned incorrect sucess code");
    }

    [TestMethod]
    public async Task ValidateProjectSettings_Failure()
    {
      var projectUid = Guid.NewGuid();
      string settings = "";
      var log = logger.CreateLogger<ProjectSettingsTests>();

      var projectSettingsRequest =
        ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid.ToString(), settings);
      var result = await ProjectSettingsValidation.RaptorValidateProjectSettings(raptorProxy, log,
        serviceExceptionHandler, projectSettingsRequest, customHeaders);

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreNotEqual(200, result.Code, "executor returned incorrect sucess code");
    }

  }
}

